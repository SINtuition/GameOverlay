﻿// <copyright file="FlaskEffectCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using ImGuiNET;

    /// <summary>
    /// For triggering a flask when flask effect is not active on player.
    /// NOTE: will not trigger a flask if flask isn't available on the slot.
    /// </summary>
    public class FlaskEffectCondition
        : BaseCondition<int>
    {
        private static int flaskSlotStatic = 1;
        private IntPtr flaskAddressCache = IntPtr.Zero;
        private string flaskBuffCache = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlaskEffectCondition"/> class.
        /// </summary>
        /// <param name="flaskSlot">flask number whos effect to use in the condition.</param>
        public FlaskEffectCondition(int flaskSlot)
            : base(OperatorEnum.NOT_CONTAINS, flaskSlot)
        {
        }

        /// <summary>
        /// Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        /// <see cref="ICondition"/> if user wants to add the condition, otherwise null.
        /// </returns>
        public static new FlaskEffectCondition Add()
        {
            ToImGui(OperatorEnum.NOT_CONTAINS, ref flaskSlotStatic);
            ImGui.SameLine();
            if (ImGui.Button($"Add##FlaskEffect"))
            {
                return new FlaskEffectCondition(flaskSlotStatic);
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Display()
        {
            ToImGui(this.conditionOperator, ref this.rightHandOperand);
            base.Display();
        }

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var flask = Core.States.InGameStateObject.ServerDataObject.
                FlaskInventory[0, this.rightHandOperand - 1];
            if (flask.Address == IntPtr.Zero)
            {
                return false;
            }

            if (flask.Address != this.flaskAddressCache)
            {
                if (flask.TryGetComponent<Base>(out var baseComponent))
                {
                    if (JsonDataHelper.FlaskNameToBuff.TryGetValue(baseComponent.ItemBaseName, out var buffName))
                    {
                        this.flaskBuffCache = buffName;
                        this.flaskAddressCache = flask.Address;
                    }
                    else
                    {
                        throw new Exception($"New flask base found {baseComponent.ItemBaseName}." +
                            $"Please let the developer know.");
                    }
                }
            }

            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Buffs>(out var buffComponent))
            {
                if (!buffComponent.StatusEffects.ContainsKey(this.flaskBuffCache))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ToImGui(OperatorEnum operation, ref int flaskSlot)
        {
            ImGui.Text($"Player {operation} flask effect of flask");
            ImGui.SameLine();
            ImGui.DragInt("##FlaskEffectFlaskSlot", ref flaskSlot, 0.05f, 1, 5);
        }
    }
}