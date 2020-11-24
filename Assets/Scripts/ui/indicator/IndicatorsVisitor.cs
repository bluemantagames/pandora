
using UnityEngine;
using System.Collections.Generic;
using Pandora;
using Pandora.Engine;

namespace Pandora.UI {
    public interface IndicatorsVisitor {
        void visit(FollowingCircleRangeIndicator indicator);
        void visit(GameObjectIndicator indicator);
        void visit(CircleRangeIndicator indicator);
        void visit(EntitiesIndicator indicator);
        void visit(LaneIndicator indicator);
    }
}