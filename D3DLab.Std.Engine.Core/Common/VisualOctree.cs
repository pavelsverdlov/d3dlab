using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;

namespace D3DLab.Std.Engine.Core.Common {
    public class EntityOctree : VisualOctree<ElementTag> , IManagerChangeSubscriber<IGraphicComponent> {
        readonly object _lock;
        public EntityOctree(BoundingBox box, int MaximumChildren) : base(box, MaximumChildren) {
            _lock = new object();
        }

        public void Change(IGraphicComponent com) {
            if (com is GeometryComponent geo) { // TODO remove this IF, remake additing to Manager base on Generic type
                if (geo.IsDisposed) {
                    Remove(geo.EntityTag);
                } else {
                    geo.BuildTreeAsync()
                        .ContinueWith(x => { lock (_lock) { Add(x.Result.Box, x.Result.EntityTag); } });
                }
            }
        }
    }


    /*
     * https://habr.com/post/334990/
     * https://www.gamedev.net/articles/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
     * https://github.com/JacekPrzemieniecki/Raytracer
     * https://github.com/Nition/UnityOctree
     * https://www.wobblyduckstudios.com/Octrees.php
     * https://www.wobblyduckstudios.com/Code/Physical.cs
     * https://www.wobblyduckstudios.com/Code/OctTree.cs
     * https://www.wobblyduckstudios.com/Code/IntersectionRecord.cs
     */

    public class VisualOctree<T> {//where T : class
        readonly OctreeNode<T> root;
        readonly Dictionary<T, OctreeItem<T>> items;

        public BoundingBox Bounds { get { return root.Bounds; } }

        public VisualOctree(BoundingBox box, int MaximumChildren) {
            root = OctreeNode<T>.CreateRoot(ref box, MaximumChildren);
            items = new Dictionary<T, OctreeItem<T>>();
        }

        public bool Add(BoundingBox box, T item) {
            var oitem = new OctreeItem<T>(ref box, item);

            if (root.Add(ref box, oitem)) {
                items.Add(item, oitem);
                return true;
            }
            return false;
        }
        public void Remove(T item) {
            items[item].SelfRemove();
            items.Remove(item);
        }

        public bool TryRemove(T item) {
            if (items.ContainsKey(item)) {
                Remove(item);
                return true;
            }
            return false;
        }

        public IEnumerable<OctreeItem<T>> GetColliding(Ray ray, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref ray, float.PositiveInfinity, predicate);
            return result;
        }

        public IEnumerable<OctreeItem<T>> GetColliding(BoundingBox box, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref box, predicate);
            return result;
        }
        public bool HasCollision(BoundingBox box, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref box, predicate);
            return result.Any();
        }


        public void Clear() {
            foreach (var i in items) {
                i.Value.SelfRemove();
            }
            items.Clear();
            root.Clear();
        }


        public void Draw() {
            root.Draw();
        }
    }

    public class OctreeNode<T>  {
        private readonly Guid key;
        public static OctreeNode<T> CreateRoot(ref BoundingBox box, int maximumChildren) {
            var node = new OctreeNode<T>(ref box, maximumChildren, null);
            return node;
        }

        const int NumChildNodes = 8;

        public BoundingBox Bounds { get; }
        public OctreeNode<T> Parent { get; }
        public int MaximumChildren { get; }
        public OctreeNode<T>[] Nodes { get { return octants; } }

        OctreeNode<T>[] octants;
        readonly HashSet<OctreeItem<T>> items;

        public bool IsLeaf() { return !octants.Any(); }
        public bool IsEmpty() { return !items.Any(); }

        public bool IsRoot { get { return Parent.IsNull(); } }

        public OctreeNode(ref BoundingBox box, int maximumChildren, OctreeNode<T> parent) {
            key = Guid.NewGuid();
            Bounds = box;
            octants = Array.Empty<OctreeNode<T>>();
            MaximumChildren = maximumChildren;
            items = new HashSet<OctreeItem<T>>();
            Parent = parent;
        }


        OctreeNode<T>[] BuildNodes() {
            Vector3 dimensions = Bounds.Maximum - Bounds.Minimum;
            Vector3 half = dimensions * 0.25f;
            Vector3 center = Bounds.GetCenter();
            var m_region = Bounds;
            var octant = new BoundingBox[NumChildNodes];
            octant[0] = new BoundingBox(m_region.Minimum, center);
            octant[1] = new BoundingBox(new Vector3(center.X, m_region.Minimum.Y, m_region.Minimum.Z), new Vector3(m_region.Maximum.X, center.Y, center.Z));
            octant[2] = new BoundingBox(new Vector3(center.X, m_region.Minimum.Y, center.Z), new Vector3(m_region.Maximum.X, center.Y, m_region.Maximum.Z));
            octant[3] = new BoundingBox(new Vector3(m_region.Minimum.X, m_region.Minimum.Y, center.Z), new Vector3(center.X, center.Y, m_region.Maximum.Z));
            octant[4] = new BoundingBox(new Vector3(m_region.Minimum.X, center.Y, m_region.Minimum.Z), new Vector3(center.X, m_region.Maximum.Y, center.Z));
            octant[5] = new BoundingBox(new Vector3(center.X, center.Y, m_region.Minimum.Z), new Vector3(m_region.Maximum.X, m_region.Maximum.Y, center.Z));
            octant[6] = new BoundingBox(center, m_region.Maximum);
            octant[7] = new BoundingBox(new Vector3(m_region.Minimum.X, center.Y, center.Z), new Vector3(center.X, m_region.Maximum.Y, m_region.Maximum.Z));
            return octant.Select(x => new OctreeNode<T>(ref x, MaximumChildren, this)).ToArray();
        }

        bool RebuildTree() {
            if (Bounds.GetDiagonal() < 4) {
                // node is too small
                return false;
            }
            var sw = new Stopwatch();
            sw.Start();
            octants = BuildNodes();
            //var nodes = new HashSet<OctreeNode<T>>();
            //sorting existing items 
            var old = items.ToList();
            items.Clear();
            for (int i = 0; i < old.Count; i++) {
                old[i].RemoveOwner(this);
                AddIntoNodes(old[i]);
            }
            sw.Stop();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddIntoNodes(OctreeItem<T> item) {
            for (int j = 0; j < octants?.Length; j++) {
                var node = octants[j];

                var cross = node.Bounds.Contains(ref item.Bound);

                if (cross == BoundingContainmentType.Contains) {
                    node.Add(item);
                    return;
                }
                if (cross == BoundingContainmentType.Intersects) {
                    node.Add(item);
                }
            }
            //items.Add(item);
        }

        void Add(OctreeItem<T> item) {
            var box = item.Bound;
            Add(ref box, item);
        }

        public bool Add(ref BoundingBox box, OctreeItem<T> item) {
            if (this.Bounds.Contains(ref box) == BoundingContainmentType.Disjoint) {
                return false;
            }

            if (items.Count >= MaximumChildren && IsLeaf()) {
                if (this.Bounds.Contains(ref item.Bound) == BoundingContainmentType.Contains) {
                    //split nodes if can
                    items.Add(item);
                    return RebuildTree();
                }
                return false;
            }
            if (!IsLeaf()) {
                AddIntoNodes(item);
            } else {
                item.AddOwner(this);
                items.Add(item);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColliding(HashSet<OctreeItem<T>> result, ref Ray ray, float maxDistance, Func<T, bool> predicate) {
            if (!Bounds.Intersects(ref ray, out float distance) || distance > maxDistance) {
                return;
            }

            foreach (var item in items) {
                if (item.Bound.Intersects(ref ray, out distance) && distance <= maxDistance && predicate(item.Item)) {
                    result.Add(item);
                }
            }

            for (var j = 0; j < octants?.Length; j++) {
                octants[j].GetColliding(result, ref ray, maxDistance, predicate);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColliding(HashSet<OctreeItem<T>> result, ref BoundingBox box, Func<T, bool> predicate) {
            if (!Bounds.Intersects(ref box)) {
                return;
            }
            foreach (var item in items) {
                if (item.Bound.Intersects(ref box) && predicate(item.Item)) {
                    result.Add(item);
                }
            }

            for (var j = 0; j < octants?.Length; j++) {
                octants[j].GetColliding(result, ref box, predicate);
            }
        }


        public void Draw() {
            //if (IsLeaf()) {
            //    Bounds.DrawBox();
            //} else {
            //    for (int i = 0; i < Nodes.Length; i++) {
            //        Nodes[i].Draw();
            //    }
            //}
            //foreach (var i in items) {
            //    i.Bound.DrawBox(global::SharpDX.Color.Beige);
            //}
        }

        public void Remove(OctreeItem<T> item) {
            items.Remove(item);
        }
        public void Clear() {
            foreach (var n in octants) {
                n.Clear();
            }
            octants = null;
            items.Clear();
        }
        public void Merge() {
            var sw = new Stopwatch();
            sw.Start();
            MergeUp(this);
            sw.Stop();
        }
        static void MergeUp(OctreeNode<T> current) {
            while (!current.IsRoot) {
                var movedItems = new HashSet<OctreeItem<T>>();
                if (!current.CanMerged(movedItems) && !movedItems.Any()) {
                    return;
                }

                //movedItems.ForEach(x => current.items.Add(x));
                //movedItems.ForEach(x => x.AddOwner(current));
                //if (current.items.Count > current.MaximumChildren) {
                //    current.RebuildTree();
                //}
                current = current.Parent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanMerged(HashSet<OctreeItem<T>> movedItems) {
            var hasOctants = octants.Any();
            var hasItems = items.Any();
            if (!hasItems && !hasOctants) {
                return true;
            }

            if (hasOctants && octants.All(x => x.CanMerged(movedItems))) {
                octants = null;
                octants = Array.Empty<OctreeNode<T>>();
            }
            //if (movedItems.Count >= MaximumChildren) {
            //    movedItems.ForEach(x => items.Add(x));
            //    movedItems.ForEach(x => x.AddOwner(this));
            //    movedItems.Clear();
            //    if (items.Count > MaximumChildren) {
            //        RebuildTree();
            //    }
            //    return false;
            //}
            //if (movedItems.Count < MaximumChildren && items.Count < MaximumChildren) {
            //    items.ForEach(x=>movedItems.Add(x));
            //    movedItems.ForEach(x => x.RemoveOwner(this));
            //    items.Clear();
            //    return true;
            //}

            return !hasItems;
        }
    }

    public class OctreeItem<T>  {
        readonly List<OctreeNode<T>> owners;
        public BoundingBox Bound;
        public T Item { get; }
        public OctreeItem(ref BoundingBox box, T item) {
            Item = item;
            Bound = box;
            owners = new List<OctreeNode<T>>();
        }
        public void AddOwner(OctreeNode<T> node) {
            owners.Add(node);
        }
        public void RemoveOwner(OctreeNode<T> node) {
            owners.Remove(node);
        }
        public void SelfRemove() {
            //var temp = owners.ToArray();
            owners.ForEach(x => x.Remove(this));
            owners.ForEach(x => x.Merge());
            owners.Clear();
        }
    }
}
