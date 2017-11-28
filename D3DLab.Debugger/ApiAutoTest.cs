using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Debugger {

    public interface IAutomationEntity {
        BoundingBox Bounds { get; }
        Matrix Matrix { get; set; }
        
    }
    public interface IAutomationViewport {
        IAutomationEntity Get(Guid guid);
        void Add(IAutomationEntity entity);
        void Remove(IAutomationEntity entity);
        bool Exist(IAutomationEntity entity);
    }


    public class AssertionComparer<TCompareType> {
        readonly IAutomationEntity entity;

        public AssertionComparer(IAutomationEntity entity) {
            this.entity = entity;
        }
    }

    public static class Assert3D {
        public static bool AreEqual(Vector3 pos1, Vector3 pos2) {
            return pos1 == pos2;
        }
        public static bool AreEqual(BoundingBox pos1, BoundingBox pos2) {
            return pos1 == pos2;
        }
    }

    public class Main {

        public void Test() {
            IAutomationViewport viewport = null;

            IAutomationEntity rec = viewport.Get(Guid.Empty);


            //move out of blank
            var blankCorner = Vector3.UnitX * 95 / 2;
            var center = Vector3.Zero;
            rec.Matrix = Matrix.Translation((blankCorner- center)*0.5f);

            //pres "save and start CAM"
            //should be popup error


        }
    }
}
