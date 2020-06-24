var firstTime = true;
var _json_data = '{"Devices": {"1":{"Name":"Ethernet 2","MAC":"0A:00:27:00:00:10","Gateway":"","IP":"192.168.1.1","Mask":"255.255.255.0","Network":"192.168.1.0","Broadcast":"192.168.1.255","Devices":[{"IP":"192.168.1.1","Hostname":"PC-SAMUEL","SNMP":[]}]},"2":{"Name":"Ethernet","MAC":"D0:17:C2:8E:9C:BE","Gateway":"192.168.0.1","IP":"192.168.0.2","Mask":"255.255.255.0","Network":"192.168.0.0","Broadcast":"192.168.0.255","Devices":[{"IP":"192.168.0.2","Hostname":"PC-SAMUEL","SNMP":{"sysDescr":"Hardware: Intel64 Family 6 Model 60 Stepping 3 AT/AT COMPATIBLE - Software: Windows Version 6.3 (Build 19041 Multiprocessor Free)","sysObjectID":"1.3.6.1.4.1.311.1.1.3.1.1","sysUpTime":"1d 22h 7m 14s 700ms","sysContact":"","sysName":"PC-SAMUEL"}},{"IP":"192.168.0.6","Hostname":"NOTEBOOK_DELL","SNMP":{"sysDescr":"Hardware: Intel64 Family 6 Model 61 Stepping 4 AT/AT COMPATIBLE - Software: Windows Version 6.3 (Build 18363 Multiprocessor Free)","sysObjectID":"1.3.6.1.4.1.311.1.1.3.1.1","sysUpTime":"12d 12h 34m 48s 970ms","sysContact":"","sysName":"NOTEBOOK_DELL"}},{"IP":"192.168.0.104","Hostname":"ESCRITORIO2-PC","SNMP":{"sysDescr":"Hardware: Intel64 Family 6 Model 28 Stepping 10 AT/AT COMPATIBLE - Software: Windows Version 6.1 (Build 7601 Multiprocessor Free)","sysObjectID":"1.3.6.1.4.1.311.1.1.3.1.1","sysUpTime":"0d 0h 8m 11s 270ms","sysContact":"Landinardo","sysName":"ESCRITORIO2-PC"}},{"IP":"192.168.0.199","Hostname":"ESCRITORIO","SNMP":{"sysDescr":"Hardware: Intel64 Family 6 Model 42 Stepping 7 AT/AT COMPATIBLE - Software: Windows Version 6.3 (Build 9600 Multiprocessor Free)","sysObjectID":"1.3.6.1.4.1.311.1.1.3.1.1","sysUpTime":"0d 0h 4m 20s 880ms","sysContact":"","sysName":"ESCRITORIO"}},{"IP":"192.168.0.7","Hostname":"SAMSUNG_LANVAN","SNMP":{"sysDescr":"Samsung Samsung M288x Series; V3.00.01.16     JAN-10-2017;Engine V1.00.14 10-16-2015;NIC 31.03.60_0.1_13-11-18;S/N NA06B07K522GN5A","sysObjectID":"1.3.6.1.4.1.236.11.5.1","sysUpTime":"0d 0h 11m 50s 590ms","sysContact":"Ivanis - Landinardo","sysName":"Samsung_LanVan"}},{"IP":"192.168.0.1","Hostname":"","SNMP":[]},{"IP":"192.168.0.4","Hostname":"EPSOND6E37B","SNMP":[]},{"IP":"192.168.0.102","Hostname":"","SNMP":[]},{"IP":"192.168.0.103","Hostname":"","SNMP":[]},{"IP":"192.168.0.101","Hostname":"","SNMP":[]},{"IP":"192.168.0.105","Hostname":"","SNMP":[]}]}},"Count": 2,"Change": "False"}';
var json_data = undefined;

function isMobile() {
    try {
        document.createEvent("TouchEvent");
        return true;
    } catch (exception) {
        return false
    }
}

$(document).ready(function () {
    $("#sidebar_toggle").on("click", function () {
        $("#sidebar").toggleClass("active");
        $('#content').toggleClass('moved');
        if (isMobile()) {
            $('#sidebar_toggle').removeClass("not_mobile");
        } else {
            $('#sidebar_toggle').addClass("not_mobile");
        }
        if ($('#sidebar_toggle').hasClass("fa-arrow-right")) {
            $('#sidebar_toggle').removeClass("fa-arrow-right");
            $('#sidebar_toggle').addClass("fa-arrow-left");
        } else {
            $('#sidebar_toggle').removeClass("fa-arrow-left");
            $('#sidebar_toggle').addClass("fa-arrow-right");
        }
    });

    $("#darkTheme").on("click", function () {
        $('#content').toggleClass('dark');
        $('.card').toggleClass('dark');
        $('.dropdown-toggle').toggleClass('dark');
        $('.progress').toggleClass('dark');
        $('#sidebar').toggleClass('dark');
        $('#sidebar .sidebar-header').toggleClass('dark');
        toggleChartDark();
    });

    if (isMobile()) {
        $(".swipe-area").swipe({
            allowPageScroll: "vertical",
            swipeRight: function (event, direction, distance, duration, fingerCount, finderData, currentDirection) {
                $("#sidebar").removeClass("active");
                $('#content').addClass('moved');
                if ($(".navbar-collapse").hasClass("navbar-collapse collapse show")) {
                    $(".navbar-toggler").click();
                }
            },
            swipeLeft: function (event, direction, distance, duration, fingerCount, finderData, currentDirection) {
                $("#sidebar").addClass("active");
                $('#content').removeClass('moved');
                if ($(".navbar-collapse").hasClass("navbar-collapse collapse show")) {
                    $(".navbar-toggler").click();
                }
            }
        });
    }

    $(document).click(function (event) {
        var clickover = $(event.target);
        var isOpen = $(".navbar-collapse").hasClass("navbar-collapse collapse show");
        if (isOpen === true && !clickover.hasClass("navbar")) {
            $(".navbar-toggler").click();
        }
    });

    Loading();

    setInterval(LoadData, 1000);
});

$('.chart').easyPieChart({
    size: 135,
    scaleColor: false,
    trackColor: "#d4d4d4",
    lineWidth: 13,
    onStep: function (from, to, percent) {
        $(this.el).find('.value').text(Math.round(percent) + "%");
    },
    barColor: function (percent) {
        if (percent >= 100) {
            return "#FF0000";
        } else if (percent <= 0) {
            return "#00FF00";
        }
        return perc2color(percent, 100, 0);
    }
});

(function ($) {
    $.fn.progressBar = function (givenValue) {
        const $this = $(this);

        function init(selector) {
            const progressValue = selector.children().attr('aria-valuenow');

            selector.children().width(progressValue + "%");
        }

        function set(selector, value) {
            var progress_bar = selector.children();
            progress_bar.removeClass('success fail active');
            progress_bar.attr('aria-valuenow', value);

            progress_bar[0].style.backgroundColor = perc2color(value, 100, 0);

            init(selector);
        }

        set($this, givenValue);
    }
}(jQuery));

var deviceData = [];
var deviceList = [];
var currentNetwork = undefined;
var currentNetworkId = undefined;
var changedNetwork = true;
var currentDevice = undefined;
function BuildSideBar() {
    var sidebarHeight = document.getElementById('sidebar').clientHeight;
    var divPos = document.getElementById('network_device_list').getBoundingClientRect();
    var scrollHeight = parseInt(sidebarHeight - divPos.top);

    if (firstTime) {

        $.ajaxSetup({
            async: false
        });
    }

    $.getJSON('deviceList.json', function (data) {
        json_data = data;
        console.log(json_data);

        //data = JSON.parse(json_data);
        if (data["empty"] == "empty") {
            return;
        }

        if (data["Change"] == "False" && !firstTime && !changedNetwork) {
            return;
        }

        deviceData = data;

        var network_list = document.getElementById("network_list");
        network_list.innerHTML = "";

        deviceList = data;

        if (firstTime) {
            for (let i = 0, j = 0; i < deviceList["Count"] + j; i++) {
                if (deviceList["Devices"][i] != undefined) {
                    currentNetwork = deviceList["Devices"][i]["Network"];
                    currentNetworkId = i;
                    break;
                } else {
                    j++;
                }
            }
        }

        for (let i = 0, j = 0; i < deviceList["Count"] + j; i++) {
            if (deviceList["Devices"][i] != undefined) {
                var listElement = document.createElement("li");

                var anchorElement = document.createElement("a");
                anchorElement.setAttribute("href", "#");
                anchorElement.setAttribute("onclick", "SetCurrentNetwork(this)");
                anchorElement.setAttribute("net-id", i);
                anchorElement.innerHTML = deviceList["Devices"][i]["Network"];
                listElement.appendChild(anchorElement);

                network_list.appendChild(listElement);

                var current_network = document.getElementById("current_network");
                current_network.setAttribute("net-id", currentNetworkId);
                current_network.innerHTML = currentNetwork;

                if (anchorElement.innerHTML == currentNetwork) {
                    listElement.classList.add("active")

                    var network_device_list = document.getElementById("network_device_list");
                    network_device_list.innerHTML = "";

                    for (let k = 0; k < deviceList["Devices"][i]["Devices"].length; k++) {
                        listElement = document.createElement("li");

                        anchorElement = document.createElement("a");
                        anchorElement.setAttribute("href", "#");
                        anchorElement.setAttribute("onclick", "SetCurrentDevice(this)");
                        anchorElement.innerHTML = deviceList["Devices"][i]["Devices"][k]["IP"];
                        listElement.appendChild(anchorElement);

                        network_device_list.setAttribute("style", "height: " + scrollHeight + "px !important");
                        network_device_list.appendChild(listElement);

                        if (firstTime && k == 0) {
                            currentDevice = deviceList["Devices"][i]["Devices"][k]["IP"];
                        }
                    }
                }
            } else {
                j++;
            }
        }

        changedNetwork = false;

        /*
            listElement.appendChild(anchorElement);
    
            var sidebar = document.getElementById("computer_list");
            var find = document.getElementById(computerList["Computers"][i]["Name"]);
            if (find == null) {
                sidebar.appendChild(listElement);
            }
    
            try {
                document.getElementById(currentComputer).parentElement.classList.add("active");
            } catch {
    
            }
        }*/
    });
}

function SetCurrentNetwork(element) {
    changedNetwork = true;

    var network_list = document.getElementById("network_list");
    var previousNetwork = network_list.getElementsByClassName("active")[0];

    previousNetwork.classList.remove("active");

    element.parentElement.classList.add("active");

    currentNetwork = element.innerText;
    currentNetworkId = element.getAttribute("net-id");

    $("#dropdown_toggle").click();

    if (currentNetwork != previousNetwork.innerText) {
        Loading();
    }
}

function Loaded() {
    $('#preloader .inner').fadeOut();
    $('#preloader').delay(0).fadeOut('slow');
    $('body').delay(250).css({ 'overflow': 'visible' });
}

function Loading() {
    $("#preloader .inner").fadeIn();
    $("body").delay(250).css({ "overflow": "hidden" });
    $("#preloader").delay(250).fadeIn(function () {
        setTimeout(LoadData, 750);

        setTimeout(Loaded, 2000); // 2000
    });
}

function LoadData() {
    BuildSideBar();

    if (firstTime) {
        getTree();

        firstTime = false;

        $.ajaxSetup({
            async: true
        });
    }
}

function arrayContainsArray(superset, subset) {
    return subset.every(function (value) {
        return (superset.indexOf(value) >= 0);
    });
}

function perc2color(perc, min, max) {
    if (perc >= 100) {
        return "#FF0000";
    } else if (perc <= 0) {
        return "#00FF00";
    }

    var base = (max - min);

    if (base == 0) { perc = 100; }
    else {
        perc = (perc - min) / base * 100;
    }
    var r, g, b = 0;
    if (perc < 50) {
        r = 255;
        g = Math.round(5.1 * perc);
    }
    else {
        g = 255;
        r = Math.round(510 - 5.10 * perc);
    }
    var h = r * 0x10000 + g * 0x100 + b * 0x1;
    return '#' + ('000000' + h.toString(16)).slice(-6);
}

function arraysEqual(a, b) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length != b.length) return false;

    // If you don't care about the order of the elements inside
    // the array, you should sort both arrays here.
    // Please note that calling sort on an array will modify that array.
    // you might want to clone your array first.
    a = a.sort();
    b = b.sort();

    for (var i = 0; i < a.length; ++i) {
        if (a[i] !== b[i]) return false;
    }
    return true;
}

function getSecondPart(str) {
    return str.split('#')[1];
}

function mapValue(value, from, to) {
    return to[0] + (value - from[0]) * (to[1] - to[0]) / (from[1] - from[0]);
}

function getTree() {
    $(function () {

        var defaultData = [
            {
                text: 'Parent 1',
                href: '#parent1',
                tags: ['4'],
                nodes: [
                    {
                        text: 'Child 1',
                        href: '#child1',
                        tags: ['2'],
                        nodes: [
                            {
                                text: 'Grandchild 1',
                                href: '#grandchild1',
                                tags: ['0']
                            },
                            {
                                text: 'Grandchild 2',
                                href: '#grandchild2',
                                tags: ['0']
                            }
                        ]
                    },
                    {
                        text: 'Child 2',
                        href: '#child2',
                        tags: ['0']
                    }
                ]
            },
            {
                text: 'Parent 2',
                href: '#parent2',
                tags: ['0']
            },
            {
                text: 'Parent 3',
                href: '#parent3',
                tags: ['0']
            },
            {
                text: 'Parent 4',
                href: '#parent4',
                tags: ['0']
            },
            {
                text: 'Parent 5',
                href: '#parent5',
                tags: ['0']
            }
        ];

        var alternateData = [
            {
                text: 'Parent 1',
                tags: ['2'],
                nodes: [
                    {
                        text: 'Child 1',
                        tags: ['3'],
                        nodes: [
                            {
                                text: 'Grandchild 1',
                                tags: ['6']
                            },
                            {
                                text: 'Grandchild 2',
                                tags: ['3']
                            }
                        ]
                    },
                    {
                        text: 'Child 2',
                        tags: ['3']
                    }
                ]
            },
            {
                text: 'Parent 2',
                tags: ['7']
            },
            {
                text: 'Parent 3',
                icon: 'glyphicon glyphicon-earphone',
                href: '#demo',
                tags: ['11']
            },
            {
                text: 'Parent 4',
                icon: 'glyphicon glyphicon-cloud-download',
                href: '/demo.html',
                tags: ['19'],
                selected: true
            },
            {
                text: 'Parent 5',
                icon: 'glyphicon glyphicon-certificate',
                color: 'pink',
                backColor: 'red',
                href: 'http://www.tesco.com',
                tags: ['available', '0']
            }
        ];

        var json = '[' +
            '{' +
            '"text": "Parent 1",' +
            '"nodes": [' +
            '{' +
            '"text": "Child 1",' +
            '"nodes": [' +
            '{' +
            '"text": "Grandchild 1"' +
            '},' +
            '{' +
            '"text": "Grandchild 2"' +
            '}' +
            ']' +
            '},' +
            '{' +
            '"text": "Child 2"' +
            '}' +
            ']' +
            '},' +
            '{' +
            '"text": "Parent 2"' +
            '},' +
            '{' +
            '"text": "Parent 3"' +
            '},' +
            '{' +
            '"text": "Parent 4"' +
            '},' +
            '{' +
            '"text": "Parent 5"' +
            '}' +
            ']';


        $('#treeview1').treeview({
            data: defaultData
        });

        var search = function (e) {
            var pattern = $('#input-search').val();
            var options = {
                ignoreCase: $('#chk-ignore-case').is(':checked'),
                exactMatch: $('#chk-exact-match').is(':checked'),
                revealResults: $('#chk-reveal-results').is(':checked')
            };
            var results = $searchableTree.treeview('search', [pattern, options]);

            var output = '<p>' + results.length + ' matches found</p>';
            $.each(results, function (index, result) {
                output += '<p>- ' + result.text + '</p>';
            });
            $('#search-output').html(output);
        }

        $('#btn-search').on('click', search);
        $('#input-search').on('keyup', search);

        $('#btn-clear-search').on('click', function (e) {
            $searchableTree.treeview('clearSearch');
            $('#input-search').val('');
            $('#search-output').html('');
        });


        var initSelectableTree = function () {
            return $('#treeview-selectable').treeview({
                data: defaultData,
                multiSelect: $('#chk-select-multi').is(':checked'),
                onNodeSelected: function (event, node) {
                    $('#selectable-output').prepend('<p>' + node.text + ' was selected</p>');
                },
                onNodeUnselected: function (event, node) {
                    $('#selectable-output').prepend('<p>' + node.text + ' was unselected</p>');
                }
            });
        };
        var $selectableTree = initSelectableTree();

        var findSelectableNodes = function () {
            return $selectableTree.treeview('search', [$('#input-select-node').val(), { ignoreCase: false, exactMatch: false }]);
        };
        var selectableNodes = findSelectableNodes();

        $('#chk-select-multi:checkbox').on('change', function () {
            console.log('multi-select change');
            $selectableTree = initSelectableTree();
            selectableNodes = findSelectableNodes();
        });

        // Select/unselect/toggle nodes
        $('#input-select-node').on('keyup', function (e) {
            selectableNodes = findSelectableNodes();
            $('.select-node').prop('disabled', !(selectableNodes.length >= 1));
        });

        $('#btn-select-node.select-node').on('click', function (e) {
            $selectableTree.treeview('selectNode', [selectableNodes, { silent: $('#chk-select-silent').is(':checked') }]);
        });

        $('#btn-unselect-node.select-node').on('click', function (e) {
            $selectableTree.treeview('unselectNode', [selectableNodes, { silent: $('#chk-select-silent').is(':checked') }]);
        });

        $('#btn-toggle-selected.select-node').on('click', function (e) {
            $selectableTree.treeview('toggleNodeSelected', [selectableNodes, { silent: $('#chk-select-silent').is(':checked') }]);
        });

        var $expandibleTree = $('#treeview-expandible').treeview({
            data: defaultData,
            onNodeCollapsed: function (event, node) {
                $('#expandible-output').prepend('<p>' + node.text + ' was collapsed</p>');
            },
            onNodeExpanded: function (event, node) {
                $('#expandible-output').prepend('<p>' + node.text + ' was expanded</p>');
            }
        });

        var findExpandibleNodess = function () {
            return $expandibleTree.treeview('search', [$('#input-expand-node').val(), { ignoreCase: false, exactMatch: false }]);
        };
        var expandibleNodes = findExpandibleNodess();

        // Expand/collapse/toggle nodes
        $('#input-expand-node').on('keyup', function (e) {
            expandibleNodes = findExpandibleNodess();
            $('.expand-node').prop('disabled', !(expandibleNodes.length >= 1));
        });

        $('#btn-expand-node.expand-node').on('click', function (e) {
            var levels = $('#select-expand-node-levels').val();
            $expandibleTree.treeview('expandNode', [expandibleNodes, { levels: levels, silent: $('#chk-expand-silent').is(':checked') }]);
        });

        $('#btn-collapse-node.expand-node').on('click', function (e) {
            $expandibleTree.treeview('collapseNode', [expandibleNodes, { silent: $('#chk-expand-silent').is(':checked') }]);
        });
    })
}

