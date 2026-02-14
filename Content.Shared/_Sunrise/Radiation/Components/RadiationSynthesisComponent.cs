/* Licensed under the BSD-3-Clause License.
 * Copyright (c) 2026 pew pewko | пью пивко.
 * This notice may not be removed from any source distribution.
 *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (
 * INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (
 * INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Contributed by pewpewko@proton.me
 */

using Robust.Shared.GameStates;


namespace Content.Shared.Radiation.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class RadiationSynthesisComponent : Component
{
    /// <summary>
    /// The minimum radiation level required for any synthesis process to activate (global threshold; individual recipes can override).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("minRadiation")]
    public float MinRadiation = 10f;

    /// <summary>
    /// The interval (in seconds) at which synthesis processes are checked and potentially executed.
    /// </summary>
    [DataField("processInterval")]
    public float ProcessInterval = 1f;  // 1 tick per second

    /// <summary>
    /// The current accumulated radiation level on the entity.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public float CurrentRadiation { get; set; } = 0f;

    /// <summary>
    /// A list of configurable synthesis recipes, each defining reactants, products, and radiation thresholds.
    /// </summary>
    [DataField("recipes")]
    public List<SynthesisRecipe> Recipes { get; set; } = new();
}

/// <summary>
/// Defines a single synthesis recipe for gas transformation under radiation.
/// </summary>
[DataDefinition]
public partial record struct SynthesisRecipe()
{
    /// <summary>
    /// Reactants: Gases to consume from the tile atmosphere (key: gas ID, value: moles required).
    /// </summary>
    [DataField("reactants")]
    public Dictionary<string, float> Reactants { get; init; } = new();

    /// <summary>
    /// Products: Gases to produce in the tile atmosphere (key: gas ID, value: moles produced).
    /// </summary>
    [DataField("products")]
    public Dictionary<string, float> Products { get; init; } = new();

    /// <summary>
    /// Minimum radiation level required for this specific recipe to activate.
    /// </summary>
    [DataField("minRadiation")]
    public float MinRadiation { get; init; } = 30f;

    /// <summary>
    /// Minimum pressure level required for this specific recipe to activate.
    /// </summary>
    [DataField("minPressure")]
    public float MinPressure { get; init; } = 0f;

    /// <summary>
    /// Maximum pressure level required for this specific recipe to activate.
    /// </summary>
    [DataField("maxPressure")]
    public float MaxPressure { get; init; } = 100000f;

    /// <summary>
    /// Minimum temperature required for this specific recipe to activate.
    /// </summary>
    [DataField("minTemperature")]
    public float MinTemperature { get; init; } = 0f;

    /// <summary>
    /// Maximum temperature required for this specific recipe to activate.
    /// </summary>
    [DataField("maxTemperature")]
    public float MaxTemperature { get; init; } = 100000f;

    /// <summary>
    /// Scaling factor for products based on excess radiation (e.g., 1.2 for a 20% bonus).
    /// </summary>
    [DataField("scaleFactor")]
    public float ScaleFactor { get; init; } = 1f;
}
