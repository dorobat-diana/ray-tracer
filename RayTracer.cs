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
            var line = new Line(light.Position, point);
            var intersection = FindFirstIntersection(line, 0.001, (light.Position - point).Length());
            if (!intersection.Valid || !intersection.Visible)
                return true;
            return intersection.T > (light.Position - point).Length()-0.001;
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color(0.2, 0.2, 0.2, 1.0); // Background color

            var image = new Image(width, height);

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // Convert pixel position (i, j) to view plane coordinates
                    double u = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    double v = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    // Compute the right vector for the view plane
                    Vector viewRight = (camera.Direction ^ camera.Up).Normalize(); // Cross product

                    // Compute the position on the view plane
                    Vector pixelPosition = (camera.Direction * camera.ViewPlaneDistance +
                                           viewRight * u +
                                           camera.Up * v).Normalize();

                    // Create a ray from the camera position through the pixel position
                    Line ray = new Line(camera.Position, pixelPosition + camera.Position);

                    // Find the first intersection with objects in geometries
                    Intersection intersection =
                        FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    // Determine pixel color using Phong shading
                    if (intersection.Valid && intersection.Visible)
                    {
                        // Start with ambient lighting component
                        var color = intersection.Material.Ambient ;

                        foreach (var light in lights)
                        {
                            // Check if the point is lit by this light
                            if (IsLit(intersection.Position, light))
                            {
                                // Position vector of the light source (L)
                                Vector L = light.Position;

                                // Position of the intersection point (V)
                                Vector V = intersection.Position;

                                // Direction from the intersection point to the light (T = L - V)
                                Vector T = (L - V).Normalize();

                                // Viewpoint position vector (C)
                                Vector C = camera.Position;

                                // Vector from the intersection point to the camera (E = C - V)
                                Vector E = (C - V).Normalize();

                                // Normal to the surface at the intersection point (N)
                                Vector N = intersection.Normal.Normalize();

                                // Compute reflection vector (R)
                                double NdotT = N * T;
                                Vector R = (2 * NdotT * N - T ).Normalize();

                                // Diffuse lighting component (only if N * T > 0)
                                if (NdotT > 0)
                                {
                                    color += intersection.Material.Diffuse * light.Diffuse * NdotT;
                                }

                                // Specular lighting component (only if E * R > 0)
                                double EdotR = E * R;
                                if (EdotR > 0)
                                {
                                    color += intersection.Material.Specular * light.Specular *
                                             Math.Pow(EdotR, intersection.Material.Shininess);
                                }
                            }

                            // Apply the light intensity (L)
                            color *= light.Intensity;
                        }
                        
                        // Set pixel color based on the final computed color
                        image.SetPixel(i, j, color);
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
