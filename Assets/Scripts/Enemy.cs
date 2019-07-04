namespace CRclone
{
    using UnityEngine;

    public class Enemy
    {
        public GameObject enemy;
        public Vector2 enemyCell;

        public Enemy(GameObject enemy, Vector2 enemyCell)
        {
            this.enemy = enemy;
            this.enemyCell = enemyCell;
        }
    }

}