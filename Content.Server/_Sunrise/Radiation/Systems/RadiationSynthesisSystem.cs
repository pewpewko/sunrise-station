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

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Server.Radiation.Components;
using Content.Shared.Radiation.Components;


namespace Content.Server.Radiation.Systems;
public sealed class RadiationSynthesisSystem : EntitySystem
{
    [Dependency] private readonly RadiationSystem _radiation = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    private float _accumulatedFrameTime;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadiationSynthesisComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, RadiationSynthesisComponent comp, ComponentStartup args)
    {
        _radiation.SetCanReceive(uid, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _accumulatedFrameTime += frameTime;
        if (_accumulatedFrameTime < 0.1f) return;
        _accumulatedFrameTime = 0f;

        var query = AllEntityQuery<RadiationSynthesisComponent, TransformComponent, RadiationReceiverComponent>();
        while (query.MoveNext(out var uid, out var synth, out var xform, out var receiver))
        {
            synth.CurrentRadiation = receiver.CurrentRadiation;

            if (synth.CurrentRadiation < synth.MinRadiation)
            {
                continue;
            }

            var mixture = _atmos.GetTileMixture(uid, excite: true);

            if (mixture is not null)
            {
                foreach (var recipe in synth.Recipes)
                {
                    if (mixture.Temperature > recipe.MaxTemperature || mixture.Temperature < recipe.MinTemperature ||
                        mixture.Pressure > recipe.MaxPressure || mixture.Pressure < recipe.MinPressure)
                        continue; // we should stay within the specified temperature and pressure limits

                    bool canReact = true;
                    foreach (var (gasId, moles) in recipe.Reactants)
                    {
                        if (mixture.GetMoles(Enum.Parse<Gas>(gasId)) < moles)
                        {
                            canReact = false;
                            break;
                        }
                    }

                    if (!canReact)
                        continue;

                    foreach (var (gasId, moles) in recipe.Reactants)
                        mixture.AdjustMoles(Enum.Parse<Gas>(gasId), -moles);

                    float excessScale =
                        MathF.Min(1f, synth.CurrentRadiation / recipe.MinRadiation); // high radiation value ensures efficient synthesis

                    foreach (var (gasId, moles) in recipe.Products)
                        mixture.AdjustMoles(Enum.Parse<Gas>(gasId), moles * excessScale * recipe.ScaleFactor);
                }
            }
        }
    }
}
