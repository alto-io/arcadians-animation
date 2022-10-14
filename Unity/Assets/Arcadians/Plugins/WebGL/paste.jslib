//https://forum.unity.com/threads/webgl-copy-paste-for-input-field-not-working.494765/
mergeInto(LibraryManager.library, {
 
  JSPasteWallet: function (sometext) {
    navigator.clipboard.readText().then(function(s) {
	  SendMessage("UINFTList", "PasteWallet", s);
	});
  },

});
