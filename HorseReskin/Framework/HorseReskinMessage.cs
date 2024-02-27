using System;

namespace HorseReskin.Framework
{
    class HorseReskinMessage
    {
        public Guid horseId;
        public int skinId;
        public HorseReskinMessage(Guid horseId, int skinId)
        {
            this.horseId = horseId;
            this.skinId = skinId;
        }
    }
}
