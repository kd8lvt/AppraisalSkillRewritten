using System;
using XRL.Core;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts.Skill
{
    [Serializable]
    public class NalathniAppraise : BaseSkill
    {
        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "OwnerGetInventoryActions");
            Object.RegisterPartEvent(this, "InvCommandAppraise");

            base.Register(Object);
        }
        private static string drams(double num)
        {
            if (num != 1) return "" + num + " drams";
            return "1 dram";
        }

        public static int Approximate(double val)
        {
            if (val <= 0) return 0;
            if (val < 1) return (int)-Approximate(1 / val);
            int tens = 0;
            while (val > 10)
            {
                val = val / 10;
                tens += 1;
            }
            double fractional = val - Math.Floor(val);
            val = Math.Floor(val);
            if (fractional >= 0.7) val += 1;
            else if (fractional >= 0.4) val += 0.5;
            while (tens > 0)
            {
                val *= 10;
                tens -= 1;
            }
            return (int)val;
        }

        public static string Quantify(int value)
        {
            if (value == 0) return "nothing";
            if (value == 1) return "a dram";
            if (value == -1) return "a dram";
            if (value == -2) return "half a dram";
            if (value > 1) return Grammar.Cardinal(value) + " drams";
            if (value < -2) return Grammar.A(Grammar.Ordinal(-value)) + " of a dram";
            return "Oops?";
        }

        public static float GetMultiplier()
        {
            GameObject body = XRLCore.Core.Game.Player.Body;
            if (!body.Statistics.ContainsKey("Ego"))
            {
                return 0.25f;
            }
            float num = (float)body.Statistics["Ego"].Modifier;
            if (body.HasPart("Persuasion_SnakeOiler"))
            {
                num += 2f;
            }
            if (body.HasEffect("Glotrot"))
            {
                num = -3f;
            }
            float num2 = 0.35f + 0.07f * num;
            if (body.HasPart("SociallyRepugnant"))
            {
                num2 /= 5f;
            }
            if (num2 > 0.95f)
            {
                num2 = 0.95f;
            }
            else if (num2 < 0.05f)
            {
                num2 = 0.05f;
            }
            return num2;
        }

        public override bool FireEvent(Event E)
        {

            if (E.ID == "OwnerGetInventoryActions")
            {
                EventParameterGetInventoryActions actions = E.GetParameter("Actions") as EventParameterGetInventoryActions;
                GameObject item = E.GetGameObjectParameter("Object");

                if (item.HasPart("Physics") && item.HasPart("Commerce") && item.GetPart<Physics>().Takeable && item.Understood())
                {
                    actions.AddAction("Mod", 'v', true, "appraise &Wv&yalue", "InvCommandAppraise");
                }
            }
            if (E.ID == "InvCommandAppraise")
            {
                GameObject item = E.GetGameObjectParameter("Object");
                int weight = item.GetPart<Physics>().Weight;
                double value = item.ValueEach * item.Count;
                string assessment = "";
                string materialEquivalent = "";
                double sellprice = value * GetMultiplier();
                if (value <= 0.01)
                {
                    assessment = item.Ithas + " got basically no trade value.";

                }
                else
                if (item.GetIntProperty("Currency", 0) != 0)
                {
                    assessment = "It's universally agreed that " + Grammar.Pluralize(item.DisplayNameOnlyDirectAndStripped) + " are worth " + Quantify((int)item.ValueEach) + " apiece.";
                }
                else
                {

                    assessment = "You could probably sell " + (item.Count > 1 ? "the lot of them" : item.it) + " for about " + Quantify(Approximate(sellprice)) + ".";
                    if (weight > 0)
                    {
                        double density = sellprice / weight;
                        assessment += "\nBy weight, that's around " + Quantify(Approximate(density)) + " per pound.";
                    }
                }

                Popup.Show(assessment, CopyScrap:true);
            }
            return true;
        }
    }
}
