namespace Pandora.Engine {
    public interface CollisionCallback {
        void Collided(EngineEntity entity, uint totalElapsed);
    }
}