using Leopotam.Ecs;

namespace EcsCore
{
    internal class EcsOneFrameSystem : IEcsRunLateSystem
    {
        private EcsFilter<EcsOneFrame> _filter;
        
        public void RunLate()
        {
            foreach (var i in _filter)
                _filter.GetEntity(i).Destroy();
        }
    }
}