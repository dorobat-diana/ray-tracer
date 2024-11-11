using System;

namespace rt
{
    public class Ellipsoid : Geometry
    {
        private Vector Center { get; }
        private Vector SemiAxesLength { get; }
        private double Radius { get; }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) 
            : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) 
            : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        // New CalculateIntersectionParameters method to return intersection distances t1 and t2
        public (double? t1, double? t2) CalculateIntersectionParameters(Line line)
        {
            // Translate ray origin to ellipsoid's local space
            Vector origin = (line.X0 - Center);
            origin.Divide(SemiAxesLength);
            Vector direction = new Vector(line.Dx);
            direction.Divide(SemiAxesLength);

            // Calculate coefficients for the quadratic equation
            double A = direction * direction;
            double B = 2 * (origin * direction);
            double C = (origin * origin) - (Radius * Radius);

            // Calculate the discriminant
            double discriminant = B * B - 4 * A * C;

            if (discriminant < 0)
                return (null, null); // No intersection

            // Solve the quadratic equation for t
            double sqrtDiscriminant = Math.Sqrt(discriminant);
            double t1 = (-B - sqrtDiscriminant) / (2 * A);
            double t2 = (-B + sqrtDiscriminant) / (2 * A);

            return (t1, t2);
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            var (t1, t2) = CalculateIntersectionParameters(line);

            if (t1 == null || t2 == null || t2 < minDist || t1 > maxDist)
                return Intersection.NONE;

            var t = t1 >= minDist ? t1.Value : t2.Value;

            var intersectionPoint = line.CoordinateToPosition(t);
            var normal = (intersectionPoint - Center) / Radius;
            normal.Normalize();

            return new Intersection(true, true, this, line, t, normal, Material, Color);
        }
    }
}
