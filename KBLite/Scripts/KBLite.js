var clickedPages = new Array();

function positionContent() {
    $('#content-container').height($(window).height() - 102)

    var directoryWidth = $('.directory:first').width();
    var subdirectoryWidth = $('.subdirectory:first').width();

    var numOfSubdirsPerRow = Math.floor(directoryWidth / subdirectoryWidth);
    var widthOfRow = numOfSubdirsPerRow * subdirectoryWidth;
    var remainingSpace = directoryWidth - widthOfRow;
    var numOfSpaces = numOfSubdirsPerRow - 1;

    var rightMargin = Math.floor((remainingSpace / numOfSpaces)) + 'px';

    var index = 1;

    $('.directory').each(function () {
        $(this).find('.subdirectory').each(function () {
            var subdirMargin = index % numOfSubdirsPerRow != 0 ? '0 ' + rightMargin + ' 0 0' : '0px';
            $(this).css('margin', subdirMargin);

            var subdirHeight = $(this).height();
            var folderElem = $(this).find('p');
            folderElem.css('margin', '0px');
            var space = subdirHeight - folderElem.outerHeight();
            var folderTopMargin = Math.floor((space / 2)) + 'px';
            folderElem.css('margin-top', folderTopMargin);

            index++;
        });

        index = 1;
    });
}

function Initialise() {
    positionContent();
}

function setContent(id) {
    $.post(getContentUrl + '?id=' + id, null,
            function (data) {
                //$('#article').html(data);
                var articleFrame = window.frames[0].document;
                articleFrame.open();
                articleFrame.write(data);
                articleFrame.close();
                bringToFront('article-container');
            }, 'text');
}

$(document).ready(Initialise);

// bring relevant article/directory into view when clicked
// by user.
function bringToFront(elem) {

    if (clickedPages.length > 0) {
        var previousElem = $('#' + clickedPages[0]);
        previousElem.css('z-index', -parseInt(previousElem.css('z-index')));
    }

    var id;

    if (elem == 'article-container') {
        id = elem;
    }
    else {
        id = $(elem).attr('name');
    }

    var targetElem = $('#' + id);

    var zIndex = elem == 'article-container' ? 10000 : -parseInt(targetElem.css('z-index'));

    targetElem.css('z-index', zIndex);

    if (targetElem.css('display') == 'none') {
        targetElem.css('display', 'block');

        if (id == 'article-container') {

            $('#article-container').height(window.frames[0].document.offsetHeight);
            $('#article').height(window.frames[0].document.body.offsetHeight + 50);
        }
    }
    else {
        targetElem.css('display', 'none');
    }

    clickedPages.unshift(id);

    $('#back-button').css('visibility', 'visible');
}

// move backward through multiple views
function rollBack(id) {
    while ((clickedPages[0] != id) && clickedPages.length != 0) {
        moveBack();
    }
}

// move a single step backward through clicked views
function moveBack() {
    var id = clickedPages.shift();
    var previousElem = $('#' + id);
    var zIndex = id == 'article-container' ? 0 : -parseInt(previousElem.css('z-index'));
    previousElem.css('z-index', zIndex);

    if (previousElem.css('display') == 'none') {
        previousElem.css('display', 'block');

        if (id == 'article-container') {
            $('#article-container').height($('#article').height());
        }
    }
    else {
        previousElem.css('display', 'none');
    }

    if (clickedPages.length > 0) {
        previousElem = $('#' + clickedPages[0]);
        previousElem.css('z-index', -parseInt(previousElem.css('z-index')));
    }

    if (clickedPages.length == 0) {
        $('#back-button').css('visibility', 'hidden');
    }
}

// Change colour on navBar that appears on the right of the
// screen.
function updateNavButton(id) {
    var navButton = $('#' + id);
    var colour = rgbToHex(navButton.css('background-color')).toUpperCase();

    if (colour == '#4878A8') {
        navButton.css('background-color', '#F07830');
    }
    else {
        navButton.css('background-color', '#4878A8');
    }
}

// Convert rgb value to hex to be correctly used
// in colour comparisons.
function rgbToHex(rgb) {
    if (rgb.search("rgb") == -1) {
        return rgb;
    } else {
        rgb = rgb.match(/^rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*(\d+))?\)$/);
        function hex(x) {
            return ("0" + parseInt(x).toString(16)).slice(-2);
        }
        return "#" + hex(rgb[1]) + hex(rgb[2]) + hex(rgb[3]);
    }
}