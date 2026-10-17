using System;
using System.Collections.Generic;
using Sweetory.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sweetory.Game
{
    // Dünyayı tek bir Mesh olarak çizer: zemin, binalar, bantlar, oklar, eşyalar.
    // Her şey renkli kare/üçgen; her karede mesh baştan kurulur. Tek GameObject, tek çizim çağrısı.
    // Hücre (x, y) dünya koordinatında [x, x+1] × [y, y+1] karesini kaplar.
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WorldRenderer : MonoBehaviour
    {
        // Inspector'da düzenlenebilir "id → renk" satırı. [Serializable] olmadan Unity bunu gösteremez.
        [Serializable]
        private struct ColorEntry
        {
            public string id;
            public Color color;
        }

        [SerializeField] private GameRunner runner;

        [SerializeField] private Color groundA = new Color32(246, 231, 207, 255);
        [SerializeField] private Color groundB = new Color32(241, 222, 194, 255);
        [SerializeField] private Color beltColor = new Color32(140, 106, 85, 255);
        [SerializeField] private Color arrowColor = new Color32(110, 80, 64, 255);
        [SerializeField] private Color progressColor = new Color32(255, 255, 255, 200);

        // Bina ve eşya renkleri. Listede olmayan id'ler için isimden türetilmiş bir renk kullanılır.
        [SerializeField]
        private ColorEntry[] colors =
        {
            new ColorEntry { id = "wheat_field", color = new Color32(201, 212, 106, 255) },
            new ColorEntry { id = "mill", color = new Color32(185, 164, 138, 255) },
            new ColorEntry { id = "oven", color = new Color32(217, 119, 59, 255) },
            new ColorEntry { id = "wheat", color = new Color32(232, 193, 74, 255) },
            new ColorEntry { id = "flour", color = new Color32(251, 248, 240, 255) },
            new ColorEntry { id = "bread", color = new Color32(181, 105, 46, 255) },
        };

        private Mesh mesh;
        private readonly Dictionary<string, Color32> colorLookup = new Dictionary<string, Color32>();

        // Inspector renklerinin mesh'e yazılacak (renk uzayı düzeltilmiş) halleri.
        private Color32 groundAVertex, groundBVertex, beltVertex, arrowVertex, progressVertex;

        // Her karede yeniden doldurulan listeler; yeniden kullanılıyor ki her karede çöp (GC) üretmesin.
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color32> vertexColors = new List<Color32>();
        private readonly List<int> triangles = new List<int>();

        private void Start()
        {
            if (runner == null) runner = GetComponent<GameRunner>();

            mesh = new Mesh { name = "WorldMesh", indexFormat = IndexFormat.UInt32 };
            mesh.MarkDynamic(); // sık değişeceğini GPU tarafına bildirir
            GetComponent<MeshFilter>().mesh = mesh;

            foreach (var entry in colors)
                colorLookup[entry.id] = ToVertexColor(entry.color);

            groundAVertex = ToVertexColor(groundA);
            groundBVertex = ToVertexColor(groundB);
            beltVertex = ToVertexColor(beltColor);
            arrowVertex = ToVertexColor(arrowColor);
            progressVertex = ToVertexColor(progressColor);

            FitCamera();
        }

        // Proje Linear renk uzayındaysa shader vertex rengini zaten linear sanar; Inspector'daki
        // (gamma/sRGB) rengi çevirmezsek her şey soluk görünür. SpriteRenderer bunu kendisi yapar, biz elle yapıyoruz.
        private static Color32 ToVertexColor(Color color) =>
            QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;

        // LateUpdate: tüm Update'lerden sonra çalışır; yani bu karenin tick'leri atıldıktan sonra çiziyoruz.
        private void LateUpdate()
        {
            Rebuild(runner.World);
        }

        private void Rebuild(World world)
        {
            vertices.Clear();
            vertexColors.Clear();
            triangles.Clear();

            // Sıra önemli: aynı mesh içinde sonra eklenen üçgen öncekinin üstüne çizilir.
            for (int y = 0; y < world.Height; y++)
                for (int x = 0; x < world.Width; x++)
                    AddRect(x, y, x + 1, y + 1, (x + y) % 2 == 0 ? groundAVertex : groundBVertex);

            foreach (var building in world.Buildings)
                DrawBuilding(building);

            foreach (var belt in world.Belts)
                DrawBelt(belt);

            foreach (var belt in world.Belts)
                if (belt.Item != null)
                    AddSquare(CellCenter(belt.Position), 0.4f, ColorFor(belt.Item.TypeId));

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(vertexColors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private void DrawBuilding(Building building)
        {
            var p = building.Position;
            AddRect(p.X + 0.05f, p.Y + 0.05f, p.X + 0.95f, p.Y + 0.95f, ColorFor(building.TypeId));

            // Üretim ilerleme çubuğu (binanın alt kenarında).
            if (building.IsCrafting)
            {
                float t = (float)building.Progress / building.Recipe.DurationTicks;
                AddRect(p.X + 0.1f, p.Y + 0.1f, p.X + 0.1f + 0.8f * t, p.Y + 0.18f, progressVertex);
            }
        }

        private void DrawBelt(Belt belt)
        {
            var p = belt.Position;
            AddRect(p.X + 0.08f, p.Y + 0.08f, p.X + 0.92f, p.Y + 0.92f, beltVertex);

            // Yön oku: ucu bant yönünde olan bir üçgen.
            var offset = belt.Direction.ToOffset();
            var dir = new Vector3(offset.X, offset.Y);
            var side = new Vector3(-dir.y, dir.x); // dir'e dik
            var c = CellCenter(p);
            AddTriangle(c + dir * 0.32f, c - dir * 0.12f + side * 0.25f, c - dir * 0.12f - side * 0.25f, arrowVertex);
        }

        private static Vector3 CellCenter(GridPos p) => new Vector3(p.X + 0.5f, p.Y + 0.5f);

        private void AddSquare(Vector3 center, float size, Color32 color)
        {
            float h = size / 2f;
            AddRect(center.x - h, center.y - h, center.x + h, center.y + h, color);
        }

        // Eksen hizalı dikdörtgen = 4 köşe, 2 üçgen.
        private void AddRect(float xMin, float yMin, float xMax, float yMax, Color32 color)
        {
            int i = vertices.Count;
            vertices.Add(new Vector3(xMin, yMin));
            vertices.Add(new Vector3(xMin, yMax));
            vertices.Add(new Vector3(xMax, yMax));
            vertices.Add(new Vector3(xMax, yMin));
            for (int k = 0; k < 4; k++) vertexColors.Add(color);

            triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
        }

        private void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color32 color)
        {
            int i = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            for (int k = 0; k < 3; k++) vertexColors.Add(color);

            triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
        }

        private Color32 ColorFor(string id)
        {
            if (colorLookup.TryGetValue(id, out var color)) return color;

            // Bilinmeyen id: isimden sabit bir renk türet (her açılışta aynı renk çıksın diye kendi hash'imiz).
            uint hash = 2166136261;
            foreach (char ch in id) hash = (hash ^ ch) * 16777619;
            color = ToVertexColor(Color.HSVToRGB((hash % 360) / 360f, 0.45f, 0.9f));
            colorLookup[id] = color;
            return color;
        }

        // Kamerayı dünyanın ortasına alır ve tüm dünya ekrana sığacak kadar uzaklaştırır.
        private void FitCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var world = runner.World;
            cam.transform.position = new Vector3(world.Width / 2f, world.Height / 2f, -10f);
            // orthographicSize = ekranın yarı yüksekliği (dünya biriminde).
            float halfHeight = world.Height / 2f;
            float halfWidth = world.Width / 2f / cam.aspect;
            cam.orthographicSize = Mathf.Max(halfHeight, halfWidth) + 0.5f;
        }
    }
}
