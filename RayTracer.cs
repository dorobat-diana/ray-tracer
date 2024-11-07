using System;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = Intersection.NONE;

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {
            // TODO: ADD CODE HERE
            return true;
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color(0.2, 0.2, 0.2, 1.0);

            var image = new Image(width, height);

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // Convert pixel position (i, j) to view plane coordinates
                    double u = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    double v = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    // Compute the right vector for the view plane
                    Vector viewRight = (camera.Direction ^ camera.Up).Normalize();  // Cross product

                    // Compute the position on the view plane
                    Vector pixelPosition = camera.Direction * camera.ViewPlaneDistance +
                                           viewRight * u +
                                           camera.Up * v;

                    // Create a ray from the camera position through the pixel position
                    Line ray = new Line(camera.Position, (pixelPosition) + camera.Position);

                    // Find the first intersection with objects in geometries
                    Intersection intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    // Determine pixel color
                    if (intersection.Valid && intersection.Visible)
                    {
                        image.SetPixel(i, j,intersection.Color);
                    }
                    else
                    {
                        // No intersection, set background color
                        image.SetPixel(i, j, background);
                    }
                }
            }

            image.Store(filename);
        }
    }
}