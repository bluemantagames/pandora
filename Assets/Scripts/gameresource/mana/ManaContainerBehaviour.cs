using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using Pandora.Events;
using Pandora.Network;
using static System.Linq.Enumerable;

namespace Pandora.Resource.Mana
{
    public class ManaContainerBehaviour : MonoBehaviour
    {
        WalletsComponent walletsComponent;
        AnimationCurve spentCurve = null;
        public int TimePassed = 0;
        public float SpentAnimationTime = 0.5f;
        public GameObject ManaTextObject;
        Text manaText;
        int lastManaBarIndex = 10;

        float? animationTimeEnd = null;

        void Start()
        {
            walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

            walletsComponent.ManaWallet.Bus.Subscribe<ManaEarned>(new EventSubscriber<ManaEvent>(OnManaEarned, "UIManaEarned"));
            walletsComponent.ManaWallet.Bus.Subscribe<ManaSpent>(new EventSubscriber<ManaEvent>(OnManaSpent, "UIManaSpent"));
            walletsComponent.ManaWallet.Bus.Subscribe<ManaUpperReserve>(new EventSubscriber<ManaEvent>(OnManaUpperReserve, "UIManaUpperReserve"));

            manaText = ManaTextObject?.GetComponent<Text>();
        }

        void OnManaEarned(ManaEvent manaEvent)
        {
            Logger.Debug($"[MANA] Mana earned successful");

            var manaEarned = manaEvent as ManaEarned;

            if (manaText != null)
                manaText.text = Mathf.FloorToInt(manaEarned.CurrentAmount / 10).ToString();

            UpdateManaUI(manaEarned.CurrentAmount, false);
        }

        void OnManaSpent(ManaEvent manaEvent)
        {
            var manaSpent = manaEvent as ManaSpent;

            if (manaText != null)
                manaText.text = Mathf.FloorToInt(manaSpent.CurrentAmount / 10).ToString();

            UpdateManaUISpent(manaSpent.CurrentAmount, manaSpent.AmountSpent);
        }

        void OnManaUpperReserve(ManaEvent manaEvent)
        {
            var manaUpperReserve = manaEvent as ManaUpperReserve;

            UpdateManaUIUpperReserve(manaUpperReserve.UpperReserve);
        }

        void UpdateManaUI(int currentMana, bool resync)
        {
            Logger.Debug($"[MANA] Starting updating mana UI with current mana {currentMana}");

            // Stop playing children if we're playing
            if (spentCurve != null) return;

            int manaIndex = ManaBarChildIndex(currentMana);

            var childMask = ChildMaskComponent(manaIndex);

            Logger.Debug($"[MANA] Updating mana UI, children {manaIndex}");

            if (!childMask.IsPlaying)
                childMask.PlayEarnAnimation();

            if (resync) Resync();
        }

        void UpdateManaUISpent(int currentMana, int spent)
        {
            animationTimeEnd = Time.time + SpentAnimationTime;

            spentCurve = AnimationCurve.EaseInOut(Time.time, currentMana + spent, animationTimeEnd.Value, currentMana);
        }

        void UpdateManaUIUpperReserve(int upperReserve)
        {
            var reservedBlocks = Mathf.FloorToInt(upperReserve / lastManaBarIndex);

            foreach (var index in Range(0, lastManaBarIndex))
            {
                var barComponent = ChildBarComponent(index);
                var maskComponent = ChildMaskComponent(index);

                var reservedBarIndexMin = lastManaBarIndex - reservedBlocks;
                var isReserved = index >= reservedBarIndexMin;

                barComponent.Reserved = isReserved;
            }

            Resync();
        }

        void Resync()
        {
            var currentMana = walletsComponent.ManaWallet.Resource;
            var manaIndex = ManaBarChildIndex(currentMana);
            var unitPercent = ManaBarChildPercent(currentMana, manaIndex);

            for (var i = 0; i < lastManaBarIndex; i++)
            {
                var manaMaskChild = ChildMaskComponent(i);

                if (i < manaIndex)
                    manaMaskChild.Percent = 1f;
                else if (i == manaIndex)
                {
                    manaMaskChild.Reset();
                    manaMaskChild.Percent = unitPercent;
                }
                else
                    manaMaskChild.Reset();
            }
        }

        void Update()
        {
            bool isManaActive = true;

            if (spentCurve != null && isManaActive)
            {
                var animationMana = Mathf.FloorToInt(spentCurve.Evaluate(Time.time));
                var manaIndex = ManaBarChildIndex(animationMana);
                var unitPercent = ManaBarChildPercent(animationMana, manaIndex);

                for (var i = manaIndex + 1; i < lastManaBarIndex; i++)
                {
                    ChildMaskComponent(i).Reset();
                }

                var manaMask = ChildMaskComponent(manaIndex);

                manaMask.StopEarnAnimation();
                manaMask.Percent = unitPercent;

                if (Time.time > animationTimeEnd)
                {
                    spentCurve = null;
                    animationTimeEnd = null;

                    Resync();
                }
            }
        }

        int ManaBarChildIndex(int mana) => System.Math.Min(mana / lastManaBarIndex, 9);

        float ManaBarChildPercent(int currentMana, int childIndex) => ((float)currentMana - (childIndex * 10)) / 10f;

        ManaBarComponent ChildBarComponent(int childIndex) =>
            transform.GetChild(childIndex).GetComponent<ManaBarComponent>();

        ManaMaskComponent ChildMaskComponent(int childIndex) =>
            transform.GetChild(childIndex).GetComponent<ManaBarComponent>().MaskComponent;
    }
}