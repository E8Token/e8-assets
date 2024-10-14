var FilePicker = {
    FileCallbackObjectName: null,
    FileCallbackMethodName: null,
    InitFilePicker: function (callbackObjectName, callbackMethodName) {
        FileCallbackObjectName = UTF8ToString(callbackObjectName);
        FileCallbackMethodName = UTF8ToString(callbackMethodName);

        var fileuploader = document.getElementById('fileuploader');
        if (!fileuploader) {
            fileuploader = document.createElement('input');
            fileuploader.setAttribute('style', 'display:none;');
            fileuploader.setAttribute('type', 'file');
            fileuploader.setAttribute('id', 'fileuploader');
            fileuploader.setAttribute('class', 'nonfocused');
            document.getElementsByTagName('body')[0].appendChild(fileuploader);

            fileuploader.onchange = function (e) {
                var files = e.target.files;
                if (files.length === 0) {
                    _ResetFileLoader();
                    return;
                }
                var file = URL.createObjectURL(files[0]);
                console.log(file);
                SendMessage(FileCallbackObjectName, FileCallbackMethodName, file);
            };
        }
    },
    PickFile: function (extensions) {
        var str = UTF8ToString(extensions);
        var fileuploader = document.getElementById('fileuploader');

        if (fileuploader === null)
            _InitFileLoader(FileCallbackObjectName, FileCallbackMethodName);

        if (str !== null || str.match(/^ *$/) === null)
            fileuploader.setAttribute('accept', str);

        fileuploader.setAttribute('class', 'focused');
        fileuploader.click();
    },
    ResetFilePicker: function () {
        var fileuploader = document.getElementById('fileuploader');

        if (fileuploader) {
            fileuploader.setAttribute('class', 'nonfocused');
        }
    },
    ExportFile: function (data, name) {
        data = UTF8ToString(data)
        name = UTF8ToString(name)
        const link = document.createElement('a')

        link.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(data))
        link.setAttribute('download', name || 'data.json')
        link.style.display = 'none'

        document.body.appendChild(link)

        link.click()

        document.body.removeChild(link)
    }
};
mergeInto(LibraryManager.library, FilePicker);