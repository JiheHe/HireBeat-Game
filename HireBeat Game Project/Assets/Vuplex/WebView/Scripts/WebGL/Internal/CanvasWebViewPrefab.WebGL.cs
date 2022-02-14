/**
* Copyright (c) 2021 Vuplex Inc. All rights reserved.
*
* Licensed under the Vuplex Commercial Software Library License, you may
* not use this file except in compliance with the License. You may obtain
* a copy of the License at
*
*     https://vuplex.com/commercial-library-license
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#if UNITY_WEBGL
using System;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    partial class CanvasWebViewPrefab {

        partial void OnInit() {

            if (_canvas?.renderMode == RenderMode.WorldSpace) {
                throw new InvalidOperationException("2D WebView for WebGL only supports Native 2D Mode, which requires that the Canvas's render mode be set to \"Screen Space - Overlay\" or \"Screen Space - Camera\", but its render mode is instead currently set to \"World Space\". Please change the Canvas's render mode to \"Screen Space - Overlay\" or \"Screen Space - Camera\".");
            }
            if (!Native2DModeEnabled) {
                Native2DModeEnabled = true;
                WebViewLogger.LogWarning("2D WebView for WebGL only supports Native 2D Mode, so CanvasWebViewPrefab.Native2DModeEnabled was automatically set to true.");
            }
        }
    }
}
#endif
