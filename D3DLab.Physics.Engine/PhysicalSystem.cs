using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using D3DLab.Physics.Engine.Bepu;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace D3DLab.Physics.Engine {
    
    #region to test

    public class SimpleThreadDispatcher : IThreadDispatcher, IDisposable {
        int threadCount;
        public int ThreadCount => threadCount;
        struct Worker {
            public Thread Thread;
            public AutoResetEvent Signal;
        }

        Worker[] workers;
        AutoResetEvent finished;

        BufferPool[] bufferPools;

        public SimpleThreadDispatcher(int threadCount) {
            this.threadCount = threadCount;
            workers = new Worker[threadCount - 1];
            for (int i = 0; i < workers.Length; ++i) {
                workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
                workers[i].Thread.IsBackground = true;
                workers[i].Thread.Start(workers[i].Signal);
            }
            finished = new AutoResetEvent(false);
            bufferPools = new BufferPool[threadCount];
            for (int i = 0; i < bufferPools.Length; ++i) {
                bufferPools[i] = new BufferPool();
            }
        }

        void DispatchThread(int workerIndex) {
            Debug.Assert(workerBody != null);
            workerBody(workerIndex);

            if (Interlocked.Increment(ref completedWorkerCounter) == threadCount) {
                finished.Set();
            }
        }

        volatile Action<int> workerBody;
        int workerIndex;
        int completedWorkerCounter;

        void WorkerLoop(object untypedSignal) {
            var signal = (AutoResetEvent)untypedSignal;
            while (true) {
                signal.WaitOne();
                if (disposed)
                    return;
                DispatchThread(Interlocked.Increment(ref workerIndex) - 1);
            }
        }

        void SignalThreads() {
            for (int i = 0; i < workers.Length; ++i) {
                workers[i].Signal.Set();
            }
        }

        public void DispatchWorkers(Action<int> workerBody) {
            Debug.Assert(this.workerBody == null);
            workerIndex = 1; //Just make the inline thread worker 0. While the other threads might start executing first, the user should never rely on the dispatch order.
            completedWorkerCounter = 0;
            this.workerBody = workerBody;
            SignalThreads();
            //Calling thread does work. No reason to spin up another worker and block this one!
            DispatchThread(0);
            finished.WaitOne();
            this.workerBody = null;
        }

        volatile bool disposed;
        public void Dispose() {
            if (!disposed) {
                disposed = true;
                SignalThreads();
                for (int i = 0; i < bufferPools.Length; ++i) {
                    bufferPools[i].Clear();
                }
                foreach (var worker in workers) {
                    worker.Thread.Join();
                    worker.Signal.Dispose();
                }
            }
        }

        public BufferPool GetThreadMemoryPool(int workerIndex) {
            return bufferPools[workerIndex];
        }
    }
    /// <summary>
    /// Bit masks which control whether different members of a group of objects can collide with each other.
    /// </summary>
    public struct SubgroupCollisionFilter {
        /// <summary>
        /// A mask of 16 bits, each set bit representing a collision group that an object belongs to.
        /// </summary>
        public ushort SubgroupMembership;
        /// <summary>
        /// A mask of 16 bits, each set bit representing a collision group that an object can interact with.
        /// </summary>
        public ushort CollidableSubgroups;
        /// <summary>
        /// Id of the owner of the object. Objects belonging to different groups always collide.
        /// </summary>
        public int GroupId;

        /// <summary>
        /// Initializes a collision filter that collides with everything in the group.
        /// </summary>
        /// <param name="groupId">Id of the group that this filter operates within.</param>
        public SubgroupCollisionFilter(int groupId) {
            GroupId = groupId;
            SubgroupMembership = ushort.MaxValue;
            CollidableSubgroups = ushort.MaxValue;
        }

        /// <summary>
        /// Initializes a collision filter that belongs to one specific subgroup and can collide with any other subgroup.
        /// </summary>
        /// <param name="groupId">Id of the group that this filter operates within.</param>
        /// <param name="subgroupId">Id of the subgroup to put this ragdoll</param>
        public SubgroupCollisionFilter(int groupId, int subgroupId) {
            GroupId = groupId;
            Debug.Assert(subgroupId >= 0 && subgroupId < 16, "The subgroup field is a ushort; it can only hold 16 distinct subgroups.");
            SubgroupMembership = (ushort)(1 << subgroupId);
            CollidableSubgroups = ushort.MaxValue;
        }

        /// <summary>
        /// Disables a collision between this filter and the specified subgroup.
        /// </summary>
        /// <param name="subgroupId">Subgroup id to disable collision with.</param>
        public void DisableCollision(int subgroupId) {
            Debug.Assert(subgroupId >= 0 && subgroupId < 16, "The subgroup field is a ushort; it can only hold 16 distinct subgroups.");
            CollidableSubgroups ^= (ushort)(1 << subgroupId);
        }

        /// <summary>
        /// Modifies the interactable subgroups such that filterB does not interact with the subgroups defined by filter a and vice versa.
        /// </summary>
        /// <param name="a">Filter from which to remove collisions with filter b's subgroups.</param>
        /// <param name="b">Filter from which to remove collisions with filter a's subgroups.</param>
        public static void DisableCollision(ref SubgroupCollisionFilter filterA, ref SubgroupCollisionFilter filterB) {
            filterA.CollidableSubgroups &= (ushort)~filterB.SubgroupMembership;
            filterB.CollidableSubgroups &= (ushort)~filterA.SubgroupMembership;
        }

        /// <summary>
        /// Checks if the filters can collide by checking if b's membership can be collided by a's collidable groups.
        /// </summary>
        /// <param name="a">First filter to test.</param>
        /// <param name="b">Second filter to test.</param>
        /// <returns>True if the filters can collide, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllowCollision(in SubgroupCollisionFilter a, in SubgroupCollisionFilter b) {
            return a.GroupId != b.GroupId || (a.CollidableSubgroups & b.SubgroupMembership) > 0;
        }

    }

    struct CarBodyProperties {
        public SubgroupCollisionFilter Filter;
        public float Friction;
    }
    struct CarCallbacks : INarrowPhaseCallbacks {
        public BodyProperty<CarBodyProperties> Properties;
        public void Initialize(Simulation simulation) {
            Properties.Initialize(simulation.Bodies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b) {
            //It's impossible for two statics to collide, and pairs are sorted such that bodies always come before statics.
            if (b.Mobility != CollidableMobility.Static) {
                return SubgroupCollisionFilter.AllowCollision(Properties[a.Handle].Filter, Properties[b.Handle].Filter);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void CreateMaterial(CollidablePair pair, out PairMaterialProperties pairMaterial) {
            pairMaterial.FrictionCoefficient = Properties[pair.A.Handle].Friction;
            if (pair.B.Mobility != CollidableMobility.Static) {
                //If two bodies collide, just average the friction.
                pairMaterial.FrictionCoefficient = (pairMaterial.FrictionCoefficient + Properties[pair.B.Handle].Friction) * 0.5f;
            }
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, NonconvexContactManifold* manifold, out PairMaterialProperties pairMaterial) {
            CreateMaterial(pair, out pairMaterial);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, ConvexContactManifold* manifold, out PairMaterialProperties pairMaterial) {
            CreateMaterial(pair, out pairMaterial);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ConvexContactManifold* manifold) {
            return true;
        }

        public void Dispose() {
            Properties.Dispose();
        }
    }
    public struct DemoPoseIntegratorCallbacks : IPoseIntegratorCallbacks {
        public Vector3 Gravity;
        public float LinearDamping;
        public float AngularDamping;
        Vector3 gravityDt;
        float linearDampingDt;
        float angularDampingDt;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public DemoPoseIntegratorCallbacks(Vector3 gravity, float linearDamping = .03f, float angularDamping = .03f) : this() {
            Gravity = gravity;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
        }

        public void PrepareForIntegration(float dt) {
            //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
            gravityDt = Gravity * dt;

            //Since this doesn't use per-body damping, we can precalculate everything.
            //linearDampingDt = MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt);
            //angularDampingDt = MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt);
            linearDampingDt = (float)Math.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt);
            angularDampingDt = (float)Math.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity) {
            //Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
            if (localInertia.InverseMass > 0) {
                velocity.Linear = (velocity.Linear + gravityDt) * linearDampingDt;
                velocity.Angular = velocity.Angular * angularDampingDt;
            }
            //Implementation sidenote: Why aren't kinematics all bundled together separately from dynamics to avoid this per-body condition?
            //Because kinematics can have a velocity- that is what distinguishes them from a static object. The solver must read velocities of all bodies involved in a constraint.
            //Under ideal conditions, those bodies will be near in memory to increase the chances of a cache hit. If kinematics are separately bundled, the the number of cache
            //misses necessarily increases. Slowing down the solver in order to speed up the pose integrator is a really, really bad trade, especially when the benefit is a few ALU ops.

            //Note that you CAN technically modify the pose in IntegrateVelocity. The PoseIntegrator has already integrated the previous velocity into the position, but you can modify it again
            //if you really wanted to.
            //This is also a handy spot to implement things like position dependent gravity or per-body damping.
        }

    }
    //The simulation has a variety of extension points that must be defined. 
    //The demos tend to reuse a few types like the DemoNarrowPhaseCallbacks, but this demo will provide its own (super simple) versions.
    //If you're wondering why the callbacks are interface implementing structs rather than classes or events, it's because 
    //the compiler can specialize the implementation using the compile time type information. That avoids dispatch overhead associated
    //with delegates or virtual dispatch and allows inlining, which is valuable for extremely high frequency logic like contact callbacks.
    unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks {
        /// <summary>
        /// Performs any required initialization logic after the Simulation instance has been constructed.
        /// </summary>
        /// <param name="simulation">Simulation that owns these callbacks.</param>
        public void Initialize(Simulation simulation) {
            //Often, the callbacks type is created before the simulation instance is fully constructed, so the simulation will call this function when it's ready.
            //Any logic which depends on the simulation existing can be put here.
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for two overlapping collidables.
        /// </summary>
        /// <param name="workerIndex">Index of the worker that identified the overlap.</param>
        /// <param name="a">Reference to the first collidable in the pair.</param>
        /// <param name="b">Reference to the second collidable in the pair.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b) {
            //Before creating a narrow phase pair, the broad phase asks this callback whether to bother with a given pair of objects.
            //This can be used to implement arbitrary forms of collision filtering. See the RagdollDemo or NewtDemo for examples.
            return true;
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for the children of two overlapping collidables in a compound-including pair.
        /// </summary>
        /// <param name="pair">Parent pair of the two child collidables.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        /// <remarks>This is called for each sub-overlap in a collidable pair involving compound collidables. If neither collidable in a pair is compound, this will not be called.
        /// For compound-including pairs, if the earlier call to AllowContactGeneration returns false for owning pair, this will not be called. Note that it is possible
        /// for this function to be called twice for the same subpair if the pair has continuous collision detection enabled; 
        /// the CCD sweep test that runs before the contact generation test also asks before performing child pair tests.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) {
            //This is similar to the top level broad phase callback above. It's called by the narrow phase before generating
            //subpairs between children in parent shapes. 
            //This only gets called in pairs that involve at least one shape type that can contain multiple children, like a Compound.
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConfigureMaterial(out PairMaterialProperties pairMaterial) {
            //The engine does not define any per-body material properties. Instead, all material lookup and blending operations are handled by the callbacks.
            //For the purposes of this demo, we'll use the same settings for all pairs.
            //(Note that there's no bounciness property! See here for more details: https://github.com/bepu/bepuphysics2/issues/3)
            pairMaterial.FrictionCoefficient = 1f;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
        }

        //Note that there is a unique callback for convex versus nonconvex types. There is no fundamental difference here- it's just a matter of convenience
        //to avoid working through an interface or casting.
        //For the purposes of the demo, contact constraints are always generated.
        /// <summary>
        /// Provides a notification that a manifold has been created for a pair. Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <param name="pairMaterial">Material properties of the manifold.</param>
        /// <returns>True if a constraint should be created for the manifold, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, NonconvexContactManifold* manifold, out PairMaterialProperties pairMaterial) {
            ConfigureMaterial(out pairMaterial);
            return true;
        }

        /// <summary>
        /// Provides a notification that a manifold has been created for a pair. Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <param name="pairMaterial">Material properties of the manifold.</param>
        /// <returns>True if a constraint should be created for the manifold, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, ConvexContactManifold* manifold, out PairMaterialProperties pairMaterial) {
            ConfigureMaterial(out pairMaterial);
            return true;
        }

        /// <summary>
        /// Provides a notification that a manifold has been created between the children of two collidables in a compound-including pair.
        /// Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <returns>True if this manifold should be considered for constraint generation, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ConvexContactManifold* manifold) {
            return true;
        }

        /// <summary>
        /// Releases any resources held by the callbacks. Called by the owning narrow phase when it is being disposed.
        /// </summary>
        public void Dispose() {
        }
    }

    //Note that the engine does not require any particular form of gravity- it, like all the contact callbacks, is managed by a callback.
    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks {
        public Vector3 Gravity;
        Vector3 gravityDt;

        /// <summary>
        /// Gets how the pose integrator should handle angular velocity integration.
        /// </summary>
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving; //Don't care about fidelity in this demo!

        public PoseIntegratorCallbacks(Vector3 gravity) : this() {
            Gravity = gravity;
        }

        /// <summary>
        /// Called prior to integrating the simulation's active bodies. When used with a substepping timestepper, this could be called multiple times per frame with different time step values.
        /// </summary>
        /// <param name="dt">Current time step duration.</param>
        public void PrepareForIntegration(float dt) {
            //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
            gravityDt = Gravity * dt;
        }

        /// <summary>
        /// Callback called for each active body within the simulation during body integration.
        /// </summary>
        /// <param name="bodyIndex">Index of the body being visited.</param>
        /// <param name="pose">Body's current pose.</param>
        /// <param name="localInertia">Body's current local inertia.</param>
        /// <param name="workerIndex">Index of the worker thread processing this body.</param>
        /// <param name="velocity">Reference to the body's current velocity to integrate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity) {
            //Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
            if (localInertia.InverseMass > 0) {
                velocity.Linear = velocity.Linear + gravityDt;
            }
        }

    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://github.com/bepu/bepuphysics2
    /// </remarks>
    public class PhysicalSystem : BaseEntitySystem , IGraphicSystem {

        readonly Simulation simulation;
        readonly BufferPool BufferPool;
        readonly IPhysicsShapeConstructor constructor;

        public PhysicalSystem() {
            BufferPool = new BufferPool();

          //  var properties = new BodyProperty<CarBodyProperties>();
            //simulation = BepuPhysics.Simulation.Create(BufferPool,
            //    new CarCallbacks() { Properties = properties },
            //    new DemoPoseIntegratorCallbacks(new Vector3(0, -10, 0)));

            simulation = Simulation.Create(BufferPool,
                new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new Vector3(0, -100, 0)));

            constructor = new PhysicsShapeConstructor(simulation, BufferPool);
        }

        protected override void Executing(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();

            //Drop a ball on a big static box.
            //simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 0), new CollidableDescription(simulation.Shapes.Add(new Box(500, 1, 500)), 0.1f)));

            try {
                var comps = new Dictionary<int, GraphicEntity>();
                //var extractor = new BepuBodiesExtractor();

                foreach (var entity in emanager.GetEntities()) {
                    var has = entity.GetComponents<PhysicalComponent>();
                    if (has.Any()) {
                        var phy = has.Single();
                        if (!phy.IsConstructed) {
                            phy.TryConstructBody(entity, constructor);
                        }
                        if (phy.IsConstructed) {
                            comps.Add(phy.ShapeIndex, entity);
                        }
                    }
                }

                simulation.Timestep(0.01f);//, ThreadDispatcher);

                for (int i = 0; i < simulation.Bodies.Sets.Length; ++i) {
                    ref var set = ref simulation.Bodies.Sets[i];
                    if (set.Allocated) {//Islands are stored noncontiguously; skip those which have been deallocated.
                        for (int bodyIndex = 0; bodyIndex < set.Count; ++bodyIndex) {
                            var shapeIndex = set.Collidables[bodyIndex].Shape;
                            if (!shapeIndex.Exists) {
                                return;
                            }
                            ref var pose = ref set.Poses[bodyIndex];
                            var pp = pose.Position;
                            var r = pose.Orientation;
                            var m = Matrix4x4.CreateFromQuaternion(new System.Numerics.Quaternion(r.X, r.Y, r.Z, r.W));

                            var entity = comps[shapeIndex.Index];
                            var com = entity.GetComponent<BepuDynamicAABBPhysicalComponent>();

                            if (!m.IsIdentity) {
                                var toZero = Matrix4x4.CreateTranslation(Vector3.Zero - com.AABBox.GetCenter());
                                m = toZero * m * toZero.Inverted();
                            }
                            var newm = m * Matrix4x4.CreateTranslation(pp - com.AABBox.GetCenter());
                            if (!newm.IsIdentity) {
                                entity
                                    .GetComponent<TransformComponent>()
                                    .MatrixWorld = newm;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                throw ex;
                //Console.WriteLine(ex.Message);
            }
        }

        class PhysicsShapeConstructor : IPhysicsShapeConstructor {
            readonly Simulation simulation;
            readonly BufferPool bufferPool;

            public PhysicsShapeConstructor(Simulation simulation, BufferPool bufferPool) {
                this.simulation = simulation;
                this.bufferPool = bufferPool;
            }

            public bool TryConstructShape(GraphicEntity entity, BepuDynamicAABBPhysicalComponent physicalComponent) {
                var hasGeo = entity.GetComponents<IGeometryComponent>();
                if (!hasGeo.Any()) {
                    return false;
                }

                var geo = hasGeo.First();

                var box = geo.Box;
                var size = box.Size();
                var fbox = new Box(size.X, size.Y, size.Z);
                fbox.ComputeInertia(1, out var sphereInertia);

                var t = simulation.Shapes.Add(fbox);
                physicalComponent.ShapeIndex = t.Index;

                simulation.Bodies.Add(BodyDescription.CreateDynamic(
                    box.GetCenter(),
                    sphereInertia,
                    new CollidableDescription(t, 0.1f),
                    new BodyActivityDescription(0.01f)));

                physicalComponent.AABBox = box;

                physicalComponent.IsConstructed = true;

                return true;
            }

            public bool TryConstructShape(GraphicEntity entity, BepuStaticAABBPhysicalComponent physicalComponent) {
                var hasGeo = entity.GetComponents<IGeometryComponent>();
                if (!hasGeo.Any()) {
                    return false;
                }
                var geo = hasGeo.First();

                if (physicalComponent.IsConstructed && geo.IsModified) {
                    //update!!
                }

                var box = geo.Box;

                var size = box.Size();
                physicalComponent.ShapeIndex = simulation.Statics.Add(
                    new StaticDescription(box.GetCenter(),
                    new CollidableDescription(simulation.Shapes.Add(new Box(size.X, size.Y, size.Z)), 0.1f))
                    );
                physicalComponent.AABBox = box;

                physicalComponent.IsConstructed = true;

                return true;
            }

            public bool TryConstructShape(GraphicEntity entity, BepuStaticMeshPhysicalComponent physicalComponent) {
                var hasGeo = entity.GetComponents<IGeometryComponent>();
                if (!hasGeo.Any()) {
                    //throw new PhysicsException("Can't construct shape, no GeometryComponent");
                    return false;
                }
                var geo = hasGeo.First();

                if(physicalComponent.IsConstructed && geo.IsModified) {
                    //update!!
                }

                bufferPool.Take<Vector3>(geo.Positions.Length, out var vertices);
                for (int i = 0; i < geo.Positions.Length; ++i) {
                    vertices[i] = geo.Positions[i];
                }
                bufferPool.Take<Triangle>((int)geo.Indices.Length / 3, out var triangles);

                
                for (var i =0; i < triangles.Length; i += 3) {
                    var ii = i * 3;
                    ref var triangle0 = ref triangles[i];
                    triangle0.A = vertices[geo.Indices[ii + 0]];
                    triangle0.B = vertices[geo.Indices[ii + 1]];
                    triangle0.C = vertices[geo.Indices[ii + 2]];
                }
                bufferPool.Return(ref vertices);

                var mesh = new Mesh(triangles, new Vector3(1, 1, 1), bufferPool);

                //var tindex = simulation.Shapes.Add(mesh);
                //physicalComponent.ShapeIndex = tindex.Index;

                //simulation.Statics.Add(
                //    new StaticDescription(
                //        Vector3.Zero,
                //        BepuUtilities.Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI / 2f),
                //        new CollidableDescription(tindex, 0.1f))
                //    );

                //physicalComponent.IsConstructed = true;

                return true;
            }
        }
    }

    interface IPhysicsShapeConstructor {
        bool TryConstructShape(GraphicEntity entity, BepuStaticAABBPhysicalComponent physicalComponent);
        bool TryConstructShape(GraphicEntity entity, BepuDynamicAABBPhysicalComponent physicalComponent);
        bool TryConstructShape(GraphicEntity entity, BepuStaticMeshPhysicalComponent physicalComponent);
        
    }
}
