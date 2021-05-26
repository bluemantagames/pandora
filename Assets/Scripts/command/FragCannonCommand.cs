using UnityEngine;
using Pandora.Engine;
using Pandora.Combat;
using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command
{
    /// <summary>The FragCannon will kill itself</summary>
    public class FragCannonCommand : MonoBehaviour, CommandBehaviour
    {
        LifeComponent lifeComponent;

        void Awake()
        {
            lifeComponent = GetComponent<LifeComponent>();
        }

        public void InvokeCommand()
        {
            var currentLife = lifeComponent.lifeValue;
            lifeComponent.AssignDamage(currentLife, new SelfDamage(gameObject));
        }

        public List<EffectIndicator> FindTargets()
        {
            throw new System.NotImplementedException();
        }
    }
}