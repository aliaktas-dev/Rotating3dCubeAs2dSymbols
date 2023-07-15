using System;
using System.Threading;
using static System.Formats.Asn1.AsnWriter;

class Program
{
    static void Main()
    {
        const int size = 16;
        const int scale = 5;
        const char block = '#';
        const double angleSpeed = 0.02;
        double angle = 0;

        // Maximize the console window and set the buffer size to match the window size
        Console.WindowHeight = Console.LargestWindowHeight;
        Console.WindowWidth = Console.LargestWindowWidth;
        Console.BufferHeight = Console.WindowHeight;
        Console.BufferWidth = Console.WindowWidth;

        // Vertices of a 3D cube centered around the origin
        var vertices = new[]
        {
            new Point3D(-1, -1, -1),
            new Point3D(-1, -1, 1),
            new Point3D(-1, 1, -1),
            new Point3D(-1, 1, 1),
            new Point3D(1, -1, -1),
            new Point3D(1, -1, 1),
            new Point3D(1, 1, -1),
            new Point3D(1, 1, 1)
        };

        // Indices of vertices for each face of the cube
        var faces = new[]
        {
            new[] {0, 1, 3, 2},
            new[] {4, 6, 7, 5},
            new[] {0, 2, 6, 4},
            new[] {1, 5, 7, 3},
            new[] {0, 4, 5, 1},
            new[] {2, 3, 7, 6}
        };

        while (true)
        {
            Console.Clear();

            // Create rotation matrix
            var rotationMatrix = Matrix4D.CreateRotationX(angle) * Matrix4D.CreateRotationY(angle);

            // Draw each face
            foreach (var face in faces)
            {
                // Transform and project vertices
                var projectedVertices = new Point2D[4];
                for (int i = 0; i < 4; i++)
                {
                    var vertex = vertices[face[i]];
                    var rotatedVertex = vertex.Transform(rotationMatrix);
                    projectedVertices[i] = new Point2D(rotatedVertex.X * scale, rotatedVertex.Z * scale);
                }

                // Draw edges
                for (int i = 0; i < 4; i++)
                {
                    var v1 = projectedVertices[i];
                    var v2 = projectedVertices[(i + 1) % 4];
                    DrawLine((int)v1.X + size, (int)v1.Y + size, (int)v2.X + size, (int)v2.Y + size, block);
                }
            }

            angle += angleSpeed;
            Thread.Sleep(100);
        }
    }

    static void DrawLine(int x1, int y1, int x2, int y2, char symbol)
    {
        int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
        int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            if (x1 >= 0 && y1 >= 0 && x1 < Console.WindowWidth && y1 < Console.WindowHeight)
            {
                // Adjust the position to the center of the console window
                Console.SetCursorPosition(x1 + Console.WindowWidth / 2 - Console.WindowWidth / 8, y1 + Console.WindowHeight / 2 - Console.WindowHeight / 8);
                Console.Write(symbol);
            }

            if (x1 == x2 && y1 == y2) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x1 += sx; }
            if (e2 <= dx) { err += dx; y1 += sy; }
        }
    }

}

struct Point2D
{
    public double X;
    public double Y;

    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }
}

struct Point3D
{
    public double X;
    public double Y;
    public double Z;

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Point3D Transform(Matrix4D matrix)
    {
        double w = X * matrix.M14 + Y * matrix.M24 + Z * matrix.M34 + matrix.M44;
        return new Point3D(
            (X * matrix.M11 + Y * matrix.M21 + Z * matrix.M31 + matrix.M41) / w,
            (X * matrix.M12 + Y * matrix.M22 + Z * matrix.M32 + matrix.M42) / w,
            (X * matrix.M13 + Y * matrix.M23 + Z * matrix.M33 + matrix.M43) / w
        );
    }
}

struct Matrix4D
{
    public double M11, M12, M13, M14;
    public double M21, M22, M23, M24;
    public double M31, M32, M33, M34;
    public double M41, M42, M43, M44;

    public static Matrix4D operator *(Matrix4D a, Matrix4D b)
    {
        return new Matrix4D
        {
            M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
            M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
            M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
            M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
            M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
            M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
            M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
            M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
            M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
            M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
            M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
            M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
            M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
            M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
            M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
            M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44
        };
    }

    public static Matrix4D CreateRotationX(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);

        return new Matrix4D
        {
            M11 = 1,
            M12 = 0,
            M13 = 0,
            M14 = 0,
            M21 = 0,
            M22 = cos,
            M23 = -sin,
            M24 = 0,
            M31 = 0,
            M32 = sin,
            M33 = cos,
            M34 = 0,
            M41 = 0,
            M42 = 0,
            M43 = 0,
            M44 = 1
        };
    }

    public static Matrix4D CreateRotationY(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);

        return new Matrix4D
        {
            M11 = cos,
            M12 = 0,
            M13 = sin,
            M14 = 0,
            M21 = 0,
            M22 = 1,
            M23 = 0,
            M24 = 0,
            M31 = -sin,
            M32 = 0,
            M33 = cos,
            M34 = 0,
            M41 = 0,
            M42 = 0,
            M43 = 0,
            M44 = 1
        };
    }
}
