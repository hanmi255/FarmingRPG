namespace Assets.Scripts.Map
{
    [System.Serializable]
    public class GridPropertyDetails
    {
        public int gridX;
        public int gridY;
        public bool isDiggable = false;
        public bool canDropItem = false;
        public bool canPlaceFurniture = false;
        public bool isPath = false;
        public bool isNPCObstacle = false;
        public int daysSinceLastDig = -1;
        public int daysSinceLastWater = -1;
        public int daysSinceLastHarvest = -1;
        public int seedItemCode = -1;
        public int growthDays = -1;

        public GridPropertyDetails()
        {
            
        }
    }
}