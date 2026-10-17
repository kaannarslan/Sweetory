using Sweetory.Core;
using UnityEngine;

namespace Sweetory.Game
{
    // Simülasyonu Unity'ye bağlar: veriyi yükler, World'ü oluşturur, sabit aralıklarla Tick() çağırır.
    // Oyun mantığı burada değil, Core'da; bu sınıf sadece "ne zaman tick atılacağını" bilir.
    public class GameRunner : MonoBehaviour
    {
        // Bir karede en fazla bu kadar tick: oyun bir an takılırsa (ör. editörde breakpoint)
        // biriken yüzlerce tick'i tek karede çalıştırıp daha da takılmasın.
        private const int MaxTicksPerFrame = 10;

        // [SerializeField]: private alan ama Inspector'da görünür ve sahne dosyasına kaydedilir.
        [SerializeField] private TextAsset gameDataJson;
        [SerializeField, Min(1)] private int worldWidth = 24;
        [SerializeField, Min(1)] private int worldHeight = 14;
        [SerializeField, Min(0.1f)] private float ticksPerSecond = 5f;

        public World World { get; private set; }
        public GameData Data { get; private set; }

        private float tickTimer;

        // Awake: nesne oluşunca, tüm Start'lardan önce bir kez çağrılır.
        // World'ü burada kuruyoruz ki WorldRenderer.Start çalıştığında hazır olsun.
        private void Awake()
        {
            Data = GameData.FromJson(gameDataJson.text);
            World = new World(worldWidth, worldHeight);
            BuildDemoLayout();
        }

        // Update: her karede bir kez. Kare süresi değişken olduğu için geçen süreyi biriktirip
        // her 1/ticksPerSecond saniyede bir tick atıyoruz → simülasyon hızı FPS'ten bağımsız.
        private void Update()
        {
            tickTimer += Time.deltaTime;
            float interval = 1f / ticksPerSecond;

            int ticks = 0;
            while (tickTimer >= interval && ticks < MaxTicksPerFrame)
            {
                tickTimer -= interval;
                World.Tick();
                ticks++;
            }
            if (ticks == MaxTicksPerFrame) tickTimer = 0f;
        }

        // Fareyle yerleştirme gelene kadar ekranda bir şey görmek için sabit örnek zincir:
        // 2 tarla → bant → değirmen → bant (köşe dönüşlü) → fırın → bant
        private void BuildDemoLayout()
        {
            PlaceBuilding("wheat_field", 2, 7);
            PlaceBuilding("wheat_field", 4, 8); // üstten aynı banda buğday bırakır
            PlaceBelt(3, 7, Direction.Right);
            PlaceBelt(4, 7, Direction.Right);
            PlaceBelt(5, 7, Direction.Right);
            PlaceBuilding("mill", 6, 7);
            PlaceBelt(7, 7, Direction.Right);
            PlaceBelt(8, 7, Direction.Down);
            PlaceBelt(8, 6, Direction.Down);
            PlaceBelt(8, 5, Direction.Right);
            PlaceBelt(9, 5, Direction.Right);
            PlaceBuilding("oven", 10, 5);
            for (int x = 11; x <= 15; x++)
                PlaceBelt(x, 5, Direction.Right);
        }

        private void PlaceBuilding(string typeId, int x, int y) =>
            World.TryPlace(Data.BuildingTypes[typeId].CreateAt(new GridPos(x, y)));

        private void PlaceBelt(int x, int y, Direction direction) =>
            World.TryPlace(new Belt(new GridPos(x, y), direction));
    }
}
