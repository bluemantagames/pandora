using UnityEngine;
using UnityEngine.Animations;
using Pandora.Events;

namespace Pandora.Resource.Mana
{
    public class ManaContainerBehaviour : MonoBehaviour
    {
        WalletsComponent walletsComponent;
        AnimationCurve spentCurve = null;
        public int TimePassed = 0;
        public float SpentAnimationTime = 0.5f;

        float? animationTimeEnd = null;

        void Start()
        {
            walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            walletsComponent.ManaWallet.Bus.Subscribe<ManaEarned>(new EventSubscriber<ManaEvent>(OnManaEarned, "UIManaEarned"));
            walletsComponent.ManaWallet.Bus.Subscribe<ManaSpent>(new EventSubscriber<ManaEvent>(OnManaSpent, "UIManaSpent"));
        }

        void OnManaEarned(ManaEvent manaEvent)
        {
            var manaEarned = manaEvent as ManaEarned;

            UpdateManaUI(manaEarned.CurrentAmount, false);
        }

        void OnManaSpent(ManaEvent manaEvent)
        {
            var manaSpent = manaEvent as ManaSpent;

            UpdateManaUISpent(manaSpent.CurrentAmount, manaSpent.AmountSpent);
        }

        void UpdateManaUI(int currentMana, bool resync)
        {
            // Stop playing children if we're playing
            if (spentCurve != null) return;

            int manaIndex = ManaBarChildIndex(currentMana);

            var childMask = ChildMaskComponent(manaIndex);

            if (!childMask.IsPlaying)
                childMask.PlayEarnAnimation();

            if (resync) Resync();
        }

        void UpdateManaUISpent(int currentMana, int spent)
        {
            animationTimeEnd = Time.time + SpentAnimationTime;

            spentCurve = AnimationCurve.Linear(Time.time, currentMana + spent, animationTimeEnd.Value, currentMana);
        }

        void Resync()
        {
            var currentMana = walletsComponent.ManaWallet.Resource;
            var manaIndex = ManaBarChildIndex(currentMana);
            var unitPercent = ManaBarChildPercent(currentMana, manaIndex);

            for (var i = 0; i < manaIndex; i++)
            {
                ChildMaskComponent(i).Percent = 1f;
            }

            var manaMask = ChildMaskComponent(manaIndex);

            manaMask.Reset();
            manaMask.Percent = unitPercent;
        }

        void Update()
        {
            if (spentCurve != null)
            {
                var animationMana = Mathf.FloorToInt(spentCurve.Evaluate(Time.time));
                var manaIndex = ManaBarChildIndex(animationMana);
                var unitPercent = ManaBarChildPercent(animationMana, manaIndex);

                for (var i = manaIndex + 1; i < 10; i++) {
                    ChildMaskComponent(i).Reset();
                }

                var manaMask = ChildMaskComponent(manaIndex);

                manaMask.Percent = unitPercent;

                if (Time.time > animationTimeEnd)
                {
                    spentCurve = null;

                    Resync();
                }
            }
        }

        int ManaBarChildIndex(int mana) => System.Math.Min(mana / 10, 9);

        float ManaBarChildPercent(int currentMana, int childIndex) => ((float)currentMana - (childIndex * 10)) / 10f;

        ManaMaskComponent ChildMaskComponent(int childIndex) => 
            transform.GetChild(childIndex).GetComponent<ManaBarComponent>().MaskComponent;
    }
}