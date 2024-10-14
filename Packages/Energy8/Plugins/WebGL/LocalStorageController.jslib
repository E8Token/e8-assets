var LocalStorageController = {
    Get: function (key) {
        var value = localStorage.getItem(UTF8ToString(key))
        if (value == null)
            value = "null"
        var bufferSize = lengthBytesUTF8(value) + 1
        var buffer = _malloc(bufferSize)
        stringToUTF8(value, buffer, bufferSize)
        return buffer
    },
    Set: function (key, value) {
        localStorage.setItem(UTF8ToString(key), UTF8ToString(value))
    },
    Remove: function (key) {
        localStorage.removeItem(UTF8ToString(key));
    }
};
mergeInto(LibraryManager.library, LocalStorageController);