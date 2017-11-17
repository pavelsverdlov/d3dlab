using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Model.Shader {
    public class CuttingPlane {
        public float PlaneDistanceFromCentre = 300;
        private float MaxDistance;
        public Plane Plane { get; set; }
        private Vector3 Normal { get; set; }
        public Color4 Color { get; set; }
        private float MinDistance { get; set; }

        public bool Activated { get; set; }

        public bool Rotate { get; set; }
        public const int offset = 1;
        public CuttingPlane(Vector3 minP,Vector3 maxP, Vector3 normal, Color4 color, float distanceToCentreOfScene){
            
            Normal = normal;
            Plane = new Plane(maxP, Normal);
            Plane = new Plane(Normal,Plane.D + offset);
            var minPlane = new Plane(minP, Normal);
           // minPlane = new Plane(Normal, minPlane.D);
           
            Color = color;
            MinDistance = distanceToCentreOfScene - minPlane.D;
            initialPoint = Plane.D;
            Activated = false;
            Rotate = false;
            PlaneDistanceFromCentre = distanceToCentreOfScene;
            this.MaxDistance = distanceToCentreOfScene - Plane.D;
            MaxDistance -= offset;
            slider = Vector3.Distance(maxP, minP);
            initialPoint = Plane.D;

        }

        readonly float initialPoint;
        float slider;
        public void UpdatePlaneDistance(double sliderValue) {
            MoveForwardPercentage = sliderValue;
          
            slider = Math.Abs(MaxDistance - MinDistance);

            // initialPoint = PlaneDistanceFromCentre - MinDistance;
            float sliderOnePercent = (slider/100f);
			Plane = new Plane(Plane.Normal, initialPoint - (float)(sliderOnePercent * MoveForwardPercentage));
        }

        public double MoveForwardPercentage { get; set; }

        public void FlipPlane(){
            var normal = Plane.Normal*-1;
			Plane = new Plane(normal, Plane.D);
        }
    }
}
