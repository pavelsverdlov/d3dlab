using D3DLab.Std.Engine.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Systems {
    public struct CameraState {
        public Vector3 LookDirection;
        public Matrix4x4 ProjectionMatrix;
        public Matrix4x4 ViewMatrix;

        public CameraState(Vector3 lookDirection, Matrix4x4 view, Matrix4x4 proj) {
            LookDirection = lookDirection;
            ProjectionMatrix = proj;
            ViewMatrix = view;
        }
    }

    public class CameraSystem : IComponentSystem {
        public void Execute(SceneSnapshot snapshot) {
            var window = snapshot.Window;
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();

            try {
                foreach (var entity in emanager.GetEntities()) {
                    foreach (var com in entity.GetComponents<CameraComponent>()) {

                        com.UpdateViewMatrix();
                        com.UpdatePerspectiveMatrix(window.Width, window.Height);

                        snapshot.UpdateCamera(new CameraState(com.LookDirection, com.ViewMatrix, com.ProjectionMatrix));
                    }
                }
            } catch (Exception ex) {
                ex.ToString();
                throw ex;
            }
        }
    }
}
