mergeInto(LibraryManager.library, {

  WSInit: function(callbackClass, callbackMethod) {
    wsInit(UTF8ToString(callbackClass), UTF8ToString(callbackMethod));
  },

  WSLogin: function(contractId) {
    wallet.signIn(UTF8ToString(contractId));
  },

  WSLogout: function() {
    wallet.signOut();
  },

  AddKey: function(publicKey, contractId) {
    addKey(UTF8ToString(publicKey), UTF8ToString(contractId));
  }
});
