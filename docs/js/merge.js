Dropzone.autoDiscover = false;
$(function () {

    var $dz = $("#dropzoneForm");
    var $downloadbtn = $("#downloadbtn");
    var $mergebtn = $("#merge-btn");
    var $successmsg = $('[name = "success-msg"]');

    $dz.dropzone({
        autoDiscover: false,
        uploadMultiple: true,
        autoProcessQueue: false,
        addRemoveLinks: true,
        acceptedFiles: "application/pdf",
        successmultiple(files, res) {
            var _dz = this;
            if (res && res.success == true && res.fileName) {

                fetch("/home/download?fileName=" + res.fileName, {
                    method: 'POST',

                })
                    .then(response => response.blob())
                    .then(response => {
                        _dz.removeAllFiles();
                        const blob = new Blob([response], { type: 'application/pdf' });
                        const downloadUrl = URL.createObjectURL(blob);
                        $downloadbtn.attr("href", downloadUrl).show();
                        $successmsg.show();
                    })

            }
        },

        init: function () {
            var _dz = this;

            this.on("addedfiles", function () {
                if (_dz.files.length > 1)
                    $mergebtn.removeAttr("disabled");
            });

            this.on("removedfile", function () {
                if (this.files.length <= 1)
                    $mergebtn.attr("disabled", "true");

            }),

                $("#merge-btn").on("click", function () {

                    _dz.processQueue();
                })
        }
    });
});