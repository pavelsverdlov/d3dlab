using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Linq;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Systems;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Animation.Formats;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Animation;
using System.Collections.Generic;

namespace D3DLab.SDX.Engine.Animation {
    public class MeshAnimationComponent : GraphicComponent, IAnimationComponent {
        public const int Slot = 3;

        static int Size = Unsafe.SizeOf<Matrix4x4>() * MaxBones;
        public const int MaxBones = 1024;
        
        public Matrix4x4[] Bones;

        public double CurrentAnimationTime { get; private set; }

        readonly string animationName;
        public MeshAnimationComponent(string animationName) {
            Bones = new Matrix4x4[MaxBones];            
            this.animationName = animationName;
        }

        public void Animate(GraphicEntity owner, TimeSpan frameRateTime) {
            var AnimatedMesh = owner.GetComponent<CMOAnimateMeshComponent>();

            if (AnimatedMesh.Bones.IsNull()) { return; }

            CurrentAnimationTime += frameRateTime.TotalMilliseconds / 1000.0f;

            //TODO: remake!
            var currentAnimation = AnimatedMesh.Animations[animationName];

            // Retrieve each bone's local transform
            for (var i = 0; i < AnimatedMesh.Bones.Count; i++) {
                Bones[i] = AnimatedMesh.Bones[i].BoneLocalTransform;
            }

            // Keep track of the last key-frame used for each bone
            CMOAnimateMeshComponent.Keyframe?[] lastKeyForBones = new CMOAnimateMeshComponent.Keyframe?[AnimatedMesh.Bones.Count];
            // Keep track of whether a bone has been interpolated
            bool[] lerpedBones = new bool[AnimatedMesh.Bones.Count];
            for (var i = 0; i < currentAnimation.Keyframes.Count; i++) {
                // Retrieve current key-frame
                var frame = currentAnimation.Keyframes[i];

                // If the current frame is not in the future
                if (frame.Time <= CurrentAnimationTime) {
                    // Keep track of last key-frame for bone
                    lastKeyForBones[frame.BoneIndex] = frame;
                    // Retrieve transform from current key-frame
                    Bones[frame.BoneIndex] = frame.Transform;
                } else {// Frame is in the future, check if we should interpolate
                    // Only interpolate a bone's key-frames ONCE
                    if (!lerpedBones[frame.BoneIndex]) {
                        // Retrieve the previous key-frame if exists
                        CMOAnimateMeshComponent.Keyframe prevFrame;
                        if (lastKeyForBones[frame.BoneIndex] != null) {
                            prevFrame = lastKeyForBones[frame.BoneIndex].Value;
                        } else {
                            continue; // nothing to interpolate
                                      // Make sure we only interpolate with 
                                      // one future frame for this bone
                        }
                        lerpedBones[frame.BoneIndex] = true;

                        // Calculate time difference between frames
                        var frameLength = frame.Time - prevFrame.Time;
                        var timeDiff = CurrentAnimationTime - prevFrame.Time;
                        float amount = (float)timeDiff / frameLength;

                        // Interpolation using Lerp on scale and translation, and Slerp on Rotation (Quaternion)
                        Vector3 t1, t2;   // Translation
                        Quaternion q1, q2;// Rotation
                        Vector3 s1, s2;     // Scale
                                            // Decompose the previous key-frame's transform

                        //SharpDX.Matrix.DecomposeUniformScale(out s1, out q1, out t1);
                        Matrix4x4.Decompose(prevFrame.Transform, out s1, out q1, out t1);

                        // Decompose the current key-frame's transform
                        //frame.Transform.DecomposeUniformScale(out s2, out q2, out t2);
                        Matrix4x4.Decompose(frame.Transform, out s2, out q2, out t2);

                        // Perform interpolation and reconstitute matrix
                        Bones[frame.BoneIndex] =
                            Matrix4x4.CreateScale((float)MathUtil.Lerp(s1.X, s2.X, amount)) *
                            Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(q1, q2, amount)) *
                            Matrix4x4.CreateTranslation(Vector3.Lerp(t1, t2, amount));
                    }
                }
            }

            // Apply parent bone transforms
            // We assume here that the first bone has no parent
            // and that each parent bone appears before children
            for (var i = 1; i < AnimatedMesh.Bones.Count; i++) {
                var bone = AnimatedMesh.Bones[i];
                if (bone.ParentIndex > -1) {
                    var parentTransform = Bones[bone.ParentIndex];
                    Bones[i] = (Bones[i] * parentTransform);
                }
            }

            // Change the bone transform from rest pose space into bone space (using the inverse of the bind/rest pose)
            for (var i = 0; i < AnimatedMesh.Bones.Count; i++) {
                Bones[i] = Matrix4x4.Transpose(AnimatedMesh.Bones[i].InvBindPose * Bones[i]);
            }

            // Check need to loop animation
            if (currentAnimation.EndTime <= CurrentAnimationTime) {
                CurrentAnimationTime = 0;
            }

            IsModified = true;
        }

        
    }
}
