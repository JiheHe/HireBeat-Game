var getImage = {
    getImageFromBrowser: function(objectNamePtr, funcNamePtr) {
      // Because unity is currently bad at JavaScript we can't use standard
      // JavaScript idioms like closures so we have to use global variables :(
      window.becauseUnitysBadWithJavacript_getImageFromBrowser =
          window.becauseUnitysBadWithJavacript_getImageFromBrowser || {
         busy: false,
         initialized: false,
         rootDisplayStyle: null,  // style to make root element visible
         root_: null,             // root element of form
         ctx_: null,              // canvas for getting image data;
      };
      var g = window.becauseUnitysBadWithJavacript_getImageFromBrowser;
      if (g.busy) {
          // Don't let multiple requests come in
          return;
      }
      g.busy = true;

      var objectName = Pointer_stringify(objectNamePtr);
      var funcName = Pointer_stringify(funcNamePtr);

      if (!g.initialized) {
          g.initialized = true;
          g.ctx = window.document.createElement("canvas").getContext("2d");

          // Append a form to the page (more self contained than editing the HTML?)
          g.root = window.document.createElement("div");
          g.root.innerHTML = [
            '<style>                                                    ',
            '.getimage {                                                ',
            '    position: absolute;                                    ',
            '    left: 0;                                               ',
            '    top: 0;                                                ',
            '    width: 100%;                                           ',
            '    height: 100%;                                          ',
            '    display: -webkit-flex;                                 ',
            '    display: flex;                                         ',
            '    -webkit-flex-flow: column;                             ',
            '    flex-flow: column;                                     ',
            '    -webkit-justify-content: center;                       ',
            '    -webkit-align-content: center;                         ',
            '    -webkit-align-items: center;                           ',
            '                                                           ',
            '    justify-content: center;                               ',
            '    align-content: center;                                 ',
            '    align-items: center;                                   ',
            '                                                           ',
            '    z-index: 2;                                            ',
            '    color: white;                                          ',
            '    background-color: rgba(0,0,0,0.8);                     ',
            '    font: sans-serif;                                      ',
            '    font-size: x-large;                                    ',
            '}                                                          ',
            '.getimage a,                                               ',
            '.getimage label {                                          ',
            '   font-size: x-large;                                     ',
            '   background-color: #666;                                 ',
            '   border-radius: 0.5em;                                   ',
            '   border: 1px solid black;                                ',
            '   padding: 0.5em;                                         ',
            '   margin: 0.25em;                                         ',
            '   outline: none;                                          ',
            '   display: inline-block;                                  ',
            '}                                                          ',
            '.getimage input {                                          ',
            '    display: none;                                         ',
            '}                                                          ',
            '</style>                                                   ',
            '<div class="getimage">                                     ',
            '    <div>                                                  ',
            '      <label for="photo">click to choose an image</label>  ',
            '      <input id="photo" type="file" accept="image/*"/><br/>',
            '      <a>cancel</a>                                        ',
            '    </div>                                                 ',
            '</div>                                                     ',
          ].join('\n');
          var input = g.root.querySelector("input");
          input.addEventListener('change', getPic);

          // prevent clicking in input or label from canceling
          input.addEventListener('click', preventOtherClicks);
          var label = g.root.querySelector("label");
          label.addEventListener('click', preventOtherClicks);

          // clicking cancel or outside cancels
          var cancel = g.root.querySelector("a");  // there's only one
          cancel.addEventListener('click', handleCancel);
          var getImage = g.root.querySelector(".getimage");
          getImage.addEventListener('click', handleCancel);

          // remember the original style
          g.rootDisplayStyle = g.root.style.display;

          window.document.body.appendChild(g.root);
      }

      // make it visible
      g.root.style.display = g.rootDisplayStyle;

      function preventOtherClicks(evt) {
          evt.stopPropagation();
      }

      function getPic(evt) {
          evt.stopPropagation();
          var fileInput = evt.target.files;
          if (!fileInput || !fileInput.length) {
              return sendError("no image selected");
          }

          var picURL = window.URL.createObjectURL(fileInput[0]);
          var img = new window.Image();
          img.addEventListener('load', handleImageLoad);
          img.addEventListener('error', handleImageError);
          img.src = picURL;
      }

      function handleCancel(evt) {
          evt.stopPropagation();
          evt.preventDefault();
          sendError("cancelled");
      }

      function handleImageError(evt) {
          sendError("Could not get image");
      }

      function handleImageLoad(evt) {
          var img = evt.target;
          window.URL.revokeObjectURL(img.src);
          if(img.width < img.height) {
            var ratio = 800/img.height;
            g.ctx.canvas.width  = img.width*ratio;
            g.ctx.canvas.height = 800;
          } else {
            var ratio = 800/img.width;
            g.ctx.canvas.height  = img.height*ratio;
            g.ctx.canvas.width = 800;
          }
          g.ctx.drawImage(img, 0, 0, g.ctx.canvas.width, g.ctx.canvas.height);

          var dataUrl = g.ctx.canvas.toDataURL();

          // free the canvas memory (could probably be zero)
          g.ctx.canvas.width  = 1;
          g.ctx.canvas.height = 1;

          sendResult(dataUrl);
          g.busy = false;
      }

      function sendError(msg) {
          sendResult("error: " + msg);
      }

      function hide() {
          g.root.style.display = "none";
      }

      function sendResult(result) {
          hide();
          g.busy = false;
          SendMessage(objectName, funcName, result);
      }
    },
};

mergeInto(LibraryManager.library, getImage);