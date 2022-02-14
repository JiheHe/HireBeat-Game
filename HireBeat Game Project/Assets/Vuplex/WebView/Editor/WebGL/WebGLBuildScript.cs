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
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;

namespace Vuplex.WebView.Editor {

    /// <summary>
    /// Modifies the compiled project's index.html file to replace instances of
    /// unityInstance.SetFullscreen(1) with window.vuplex.SetFullscreen(1).
    /// https://support.vuplex.com/articles/webgl-fullscreen
    /// </summary>
    public class WebGLBuildScript {

        [PostProcessBuild(700)]
        public static void OnPostProcessBuild(BuildTarget target, string builtProjectPath) {

            if (target != BuildTarget.WebGL) {
                return;
            }
            #if !VUPLEX_WEBGL_DISABLE_FULLSCREEN_OVERRIDE
                var indexHtmlFilePath = Path.Combine(builtProjectPath, "index.html");
                if (!File.Exists(indexHtmlFilePath)) {
                    return;
                }
                var indexHtmlFileText = File.ReadAllText(indexHtmlFilePath);
                #if UNITY_2019_1_OR_NEWER
                    var stringToReplace = "unityInstance.SetFullscreen(1)";
                #else
                    // The template for Unity 2018 uses the variable name gameInstance instead.
                    var stringToReplace = "gameInstance.SetFullscreen(1)";
                #endif
                var updatedIndexHtmlFileText = indexHtmlFileText.Replace(stringToReplace, "window.vuplex.SetFullscreen(1)/*https://support.vuplex.com/articles/webgl-fullscreen*/");
                File.WriteAllText(indexHtmlFilePath, updatedIndexHtmlFileText);
            #endif
        }
    }
}
#endif
