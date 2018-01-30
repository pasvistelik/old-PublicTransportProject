﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mainPage.aspx.cs" Inherits="WebApi.mainPage" %>

<!DOCTYPE html>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Public Transport Project</title>
    
    <link type="text/css" rel="stylesheet" href="css/demo.css" />
    <link type="text/css" rel="stylesheet" href="jQuery.mmenu-master/dist/css/jquery.mmenu.all.css" />

    <script type="text/javascript" src="http://code.jquery.com/jquery-2.2.0.js"></script><!---->
    <script type="text/javascript" src="jQuery.mmenu-master/dist/js/jquery.mmenu.all.min.js"></script>
    <style>
        * {
            padding: 0px;
            margin: 0px;
        }

        html, body, #page, .content, #map {
            width: 100%;
            height: 100%;
        }
        body {
        overflow:hidden;
        }
        #my_elements div div{
        margin-left:10px !important;
        margin-bottom: 1px;
        border: 1px solid lightgray;
        }
        #my_elements div p {
        margin-top:10px;    
        }
        .contextmenu{
          visibility:hidden;
          background:#ffffff;
          border:1px solid #8888FF;
          z-index: 10;
          position: relative;
          width: 220px;
        }
        .contextmenu div{
        padding-left: 5px
        }
        #customization_result {
        font-size:12px;
        overflow-y:scroll;
        }
        /*#results_and_customization, */#start_route {
        display:none;
        }
        #results, #customization {
        display:none;
        }
        .checkbox_elem {
        white-space:nowrap;
        display:block;
        }
        .checkbox_elem input {
        margin-right:1em;
        }
        /*label[input[disabled]] {
        color:darkgray;
        }*/
        .block_elem {
        display:block;
        }
        .block_elem input {
        margin-right:1em;
        width: 200px;
        }
        #waiting_route {
        display:none;
        }
        .resultLink {
        display: block;
        text-decoration: none;
        user-select: none;
        }
        .resultLink:hover {
        text-decoration: none;
        background-color: gainsboro;
        cursor: pointer;
        }
        .effectivityPercentGold {
        background-color:goldenrod;
        padding: 1px 10px 1px 10px;
        margin: 1px;
        font-size:10px;
        border-radius:25px;
        color: white;
        cursor:help;
        display:inline-block;
        }
        .effectivityPercentPlatinum {
        background-color:indianred;
        padding: 1px 10px 1px 10px;
        margin: 1px;
        font-size:10px;
        border-radius:25px;
        color: white;
        cursor:help;
        display:inline-block;
        }
        .effectivityPercentSilver {
        background-color:silver;
        padding: 1px 10px 1px 10px;
        margin: 1px;
        font-size:10px;
        border-radius:25px;
        color: dimgray;
        cursor:help;
        display:inline-block;
        }
    </style>

    <script type="text/javascript">
        var apiPublicTransportServer = 'http://' + document.domain.toString() + ':6967/api/';//localhost
    </script>
    <script async defer src="https://maps.googleapis.com/maps/api/js?key=AIzaSyAKgX75Si-B3ewN855hbax8mJzIsDRThAU"></script>
    <script type="text/javascript">
        var findedOptimalWays = null;
        var totalTimePercent = 1;
        var totalGoingTimePercent = 1;
        var totalTransportChangingCountPercent = 1;
        function xhrGet(url) {
            var deferred = new $.Deferred();
            //deferred.addCallback(function(result) { return result; });
            var xhr = new XMLHttpRequest();
            xhr.open("GET", url, true);
            xhr.onreadystatechange = function() {
                if (xhr.readyState!=4) return;
                if (xhr.status==200) {
                    deferred.resolve(xhr.responseText);
                } else {
                    //deferred.errback(xhr.statusText);
                }
            }
            xhr.send(null);
            return deferred;
        }
        function xhrGetXML(url) {
            var deferred = new $.Deferred();
            //deferred.addCallback(function(result) { return result; });
            var xhr = new XMLHttpRequest();
            xhr.open("GET", url, true);
            xhr.onreadystatechange = function() {
                if (xhr.readyState!=4) return;
                if (xhr.status==200) {
                    deferred.resolve(xhr.responseXML);
                } else {
                    //deferred.errback(xhr.statusText);
                }
            }
            xhr.send(null);
            return deferred;
        }
        function xhrPut(url, value) {
            var deferred = new $.Deferred();
            var xhr = new XMLHttpRequest();
            xhr.open("PUT", url, true);
            xhr.setRequestHeader("Content-type", "application/json; charset=utf-8");
            var tmp = JSON.stringify(value);
            //xhr.setRequestHeader("Accept", "application/json; charset=utf-8");
            //xhr.setRequestHeader("Content-Length", tmp.length);
            //xhr.setRequestHeader('value', tmp);
            xhr.send(tmp);
            xhr.onreadystatechange = function () {
                if (xhr.readyState != 4) return;
                if (xhr.status == 200) {
                    deferred.resolve(xhr.responseText);
                } else {
                    //deferred.errback(xhr.statusText);
                }
            }
            //xhr.send();
            return deferred;
        }

        function getPointsArr(my_url) {
            var resultArr = new Array();
            var deferred = new $.Deferred();

            var xhr5 = xhrGetXML(my_url);
            xhr5.done(function(res1)
            {
                var route2_osm_xml = res1;
                var ways2_fragments = route2_osm_xml.getElementsByTagName("nd");
                for(var t = 0, need = ways2_fragments.length-1; t < ways2_fragments.length; t++)
                {

                    var xhr6 = xhrGetXML("http://api.openstreetmap.org/api/0.6/node/"+ways2_fragments[t].getAttribute("ref"));
                    xhr6.done(function(res)
                    {
                        var route3_osm_xml = res;
                        var ways3_fragments = route3_osm_xml.getElementsByTagName("node");

                        var resultElement = {
                            lat: parseFloat(ways3_fragments[0].getAttribute("lat")),
                            lng: parseFloat(ways3_fragments[0].getAttribute("lon"))
                        };
                        resultArr.push(resultElement);
                        need--;
                        if(need == 0)
                        {
                            //console.log("All:\n"+JSON.stringify(resultArr));
                            deferred.resolve(resultArr);
                        }
                    });

                }
            });

            return deferred;
        }

        var locations;
        var map;

        function loadAndViewStationInfo(current_station_code)
        {
            var xhr4 = xhrGet(apiPublicTransportServer + "stations/" + current_station_code);
            xhr4.done(function (res5) {
                var current_station = JSON.parse(res5);
                $('#selected_element_name').text("[" + current_station.hashcode + "] " + current_station.name);
            });

            var xhr3 = xhrGet(apiPublicTransportServer + "stations/" + current_station_code + "/routes");
            xhr3.done(function (res4) {
                var routes_json = res4;
                var routes = JSON.parse(routes_json);
                if (routes == null || routes.length == 0) {
                    $("#loading_text").css({ "display": "none" });
                }
                else {
                    for (var i = 0, need = routes.length - 1; i < routes.length; i++) {
                        var xhr = xhrGet(apiPublicTransportServer + "routes/" + routes[i]);
                        xhr.done(function (res3) {
                            var route_json = res3;
                            var route = JSON.parse(route_json);


                            if(route != null)
                            {
                                $('#routes_on_station').append(
                                    jQuery('<li/>', { id: 'test_0' }).append(
                                        jQuery('<span/>', {
                                            css: {
                                                cursor: 'pointer',
                                            },
                                            text: route.type + " № " + route.osm_num + ": " + route.from + " - " + route.to,
                                            click: function () {
                                                var xhr2 = xhrGetXML("http://api.openstreetmap.org/api/0.6/relation/" + route.osm_id[0]);
                                                xhr2.done(function (res2) {
                                                    var route_osm_xml = res2;
                                                    var ways_fragments = route_osm_xml.getElementsByTagName("member");
                                                    for (var j = 0; j < ways_fragments.length; j++) {
                                                        if (ways_fragments[j].getAttribute("type") == "way") {
                                                            var myArrRes = getPointsArr("http://api.openstreetmap.org/api/0.6/way/" + ways_fragments[j].getAttribute("ref"));
                                                            myArrRes.done(function (res9) {
                                                                var flightPath = new google.maps.Polyline({
                                                                    path: res9,
                                                                    geodesic: true,
                                                                    strokeColor: '#FF0000',//#FCD6A4
                                                                    strokeOpacity: 1.0,
                                                                    strokeWeight: 5
                                                                });

                                                                flightPath.setMap(map);
                                                                flightPath.addListener('click', function () { flightPath.setMap(null); });
                                                            });
                                                        }
                                                    }
                                                });
                                            }
                                        })
                                            /*)
                                        )*/
                                    )
                                );

                            }
                            need--;
                            if (need <= 0) $("#loading_text").css({ "display": "none" });
                        
                        });
                    }

                }
            });
        }

        function initialize() {
            // Задаем свойства карты и инициализируем ее

            var options = {
                scrollwheel: true,
                scaleControl: true,
                mapTypeControlOptions: { style: google.maps.MapTypeControlStyle.DROPDOWN_MENU }
            }

            map = new google.maps.Map(document.getElementById("map"), options);
            map.setCenter(new google.maps.LatLng(53.673311, 23.834641));
            map.setZoom(12);

            // Задаем слой с OSM

            var openStreet = new google.maps.ImageMapType({
                getTileUrl: function (ll, z) {
                    var X = ll.x % (1 << z);  // wrap
                    return "http://tile.openstreetmap.org/" + z + "/" + X + "/" + ll.y + ".png";
                },
                tileSize: new google.maps.Size(256, 256),
                isPng: true,
                maxZoom: 18,
                name: "OSM",
                alt: "Слой с Open Streetmap"
            });

            //Добавляем слои к карте

            map.mapTypes.set('osm', openStreet);
            map.setMapTypeId('osm');

            map.setOptions({
                mapTypeControlOptions: {
                    mapTypeIds: [
                        'osm',
                        google.maps.MapTypeId.ROADMAP,
                        google.maps.MapTypeId.TERRAIN,
                        google.maps.MapTypeId.SATELLITE,
                        google.maps.MapTypeId.HYBRID
                    ],
                    style: google.maps.MapTypeControlStyle.DROPDOWN_MENU
                }
            });

            var myLatitude, myLongitude;
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                    myLatitude = position.coords.latitude;
                    myLongitude = position.coords.longitude;

                    setStartOptimalRoutePoint({ lat: myLatitude, lng: myLongitude });

                    //alert(latitude + ' ' + longitude);
                    var image = new google.maps.MarkerImage('gps.png',
                        // This marker is 20 pixels wide by 32 pixels tall.
                        null,
                        // The origin for this image is 0,0.
                        new google.maps.Point(0, 0),
                        // The anchor for this image is the base of the flagpole at 0,32.
                        new google.maps.Point(22, 25)
                    );
                    var beachMarker = new google.maps.Marker({
                        position: { lat: myLatitude, lng: myLongitude },
                        map: map,
                        icon: image,
                        title: 'Your destination.'
                    });

                });
            }
            else {
                alert("Geolocation API не поддерживается в вашем браузере");
            }


            var gettingStations = xhrGet(apiPublicTransportServer + "stations/");
            gettingStations.done(function (returnedStationsJSON) {
                var allStations = JSON.parse(returnedStationsJSON.toString());
                locations = new Array();
                for (var indexser_0 = 0; indexser_0 < allStations.length; indexser_0++) {
                    locations.push({
                        lat: parseFloat(allStations[indexser_0].xCoord),
                        lng: parseFloat(allStations[indexser_0].yCoord)
                    });
                }






               
                var markers = locations.map(function (location, i) {
                    var tmpMarker = new google.maps.Marker({
                        position: location,
                        icon: 'bus-stop-pin.png',
                        title: allStations[i].hashcode.toString(),
                        /*{
                      url: 'http://pdd.ua/r/r/AC3D1910-D029-43A8-826E-9B0147E9681C/5.41_b.gif',//'bus-stop-location.png',
                      scaledSize: new google.maps.Size(30, 45)
                  }*/
                        //label: labels[i % labels.length]
                    });

                    tmpMarker.addListener('click', function () {

                        $('#viev_menu_link').click();
                        $('#routes_on_station').text("");
                        $("#loading_text").css({ "display": "block" });
                        $("#not_selected_text").css({ "display": "none" });
                        
                        loadAndViewStationInfo(allStations[i].hashcode);
                    });
                    return tmpMarker;
                });

                // Add a marker clusterer to manage the markers.
                var markerCluster = new MarkerClusterer(map, markers,
                    {
                        imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m'
                    });


            });

            $('#tsttst').text("");
            $('#tsttst').append(
                jQuery('<li/>').append(
                    jQuery('<span/>', {
                        css: {
                            cursor: 'pointer',
                        },
                        id: 'test_t',
                        text: "click me...",
                        click: function () {
                            //alert("sending data...");
                            var xhr2 = xhrPut(apiPublicTransportServer + "stations/S157A8E088DD", "MY TEXT 123 !%^&*");
                            xhr2.done(function (res2) {
                                //alert(res2.toString());
                                $('#test_t').append(jQuery('<span/>', { text: res2}));
                            });
                        }
                    })
                        /*)
                    )*/
                )
            );
            




            map.addListener('click', function () {
                $('.contextmenu').remove();
                //alert('123');
            });

            map.addListener('rightclick', function (event) {
                //ev.preventDefault();
                //alert('success!');
                showContextMenu(event.latLng);
                return false;
            });

            /*map.addListener('bounds_changed', function () {
                var r = getStarionsInRectangle(map.getBounds().getSouthWest(), map.getBounds().getNorthEast());
                r.done(function (res2) {
                    //alert(res2);

                });
            });*/

            $("#map").height(($("#map").outerHeight(true) - $(".header").outerHeight(true)).toString());

        }

        function countOptimalRoute(from, to)
        {
            var r = getOptimalRoute(from, to);
            r.done(function (res2) {
                //alert(res2);
                $('#tsttst').text("");
                var steps = JSON.parse(res2.toString());
                if(steps != null)
                {
                    for (var i = 0; i < steps.length; i++) {
                        $('#tsttst').append(
                            jQuery('<li/>').append(
                                jQuery('<span/>', {
                                    
                                    css: {
                                        cursor: 'pointer',
                                    },
                                    text: steps[i],
                                    click: function () {
                                        //alert("sending data...");
                                        var xhr2 = xhrPut(apiPublicTransportServer + "stations/S157A8E088DD", "MY TEXT 123 !%^&*");
                                        xhr2.done(function (res2) {
                                            //alert(res2.toString());
                                            $('#test_t').append(jQuery('<span/>', { text: res2 }));
                                        });
                                    }
                                })
                                    /*)
                                )*/
                            )
                        );
                    }
                }
                $('#viev_menu_link').click();
            });
        }
        function getOptimalRoute(from, to) {
            var deferred = new $.Deferred();
            var myCrdsFrom = from.lat().toFixed(4) + ',' + from.lng().toFixed(4);
            var myCrdsTo = to.lat().toFixed(4) + ',' + to.lng().toFixed(4);
            //alert(myCrds);
            var xhr2 = xhrGet(apiPublicTransportServer + "points?from=" + myCrdsFrom + "&to=" + myCrdsTo);
            xhr2.done(function (res2) {
                //alert(res2);
                //return res2;
                deferred.resolve(res2);
            });
            return deferred;
        }

        function getStarionsInRectangle(from, to) {
            var deferred = new $.Deferred();
            var myCrds = from.lat().toFixed(4) + ',' + from.lng().toFixed(4) + ',' + to.lat().toFixed(4) + ',' + to.lng().toFixed(4);
            //alert(myCrds);
            var xhr2 = xhrGet(apiPublicTransportServer + "stations?coords=" + myCrds);
            xhr2.done(function (res2) {
                //alert(res2);
                //return res2;
                deferred.resolve(res2);
            });
            return deferred;
        }

        var startOptimalRoutePoint = null;
        var finalOptimalRoutePoint = null;
        var widthMaximized = true;
        var canRealTimeRecounting = true;
        function tryCountOptimalRoute()
        {
            if (startOptimalRoutePoint != null && finalOptimalRoutePoint != null) {
                var southWest;// = new google.maps.LatLng(36.90731625763393, -86.51778523864743);
                var northEast;// = new google.maps.LatLng(37.02763411292923, -86.37183015289304);
                
                //alert(JSON.stringify(startOptimalRoutePoint.getPosition()).toString());
                if (startOptimalRoutePoint.getPosition().lat() > finalOptimalRoutePoint.getPosition().lat()) {
                    if (startOptimalRoutePoint.getPosition().lng() > finalOptimalRoutePoint.getPosition().lng()) {
                        northEast = startOptimalRoutePoint.getPosition();
                        southWest = finalOptimalRoutePoint.getPosition();
                    }
                    else {
                        
                        northEast = new google.maps.LatLng(startOptimalRoutePoint.getPosition().lat(), finalOptimalRoutePoint.getPosition().lng());
                        southWest = new google.maps.LatLng(finalOptimalRoutePoint.getPosition().lat(), startOptimalRoutePoint.getPosition().lng());
                    }
                }
                else {
                    if (startOptimalRoutePoint.getPosition().lng() > finalOptimalRoutePoint.getPosition().lng()) {
                        southWest = finalOptimalRoutePoint.getPosition();
                        northEast = startOptimalRoutePoint.getPosition();
                    }
                    else {
                        southWest = new google.maps.LatLng(finalOptimalRoutePoint.getPosition().lat(), startOptimalRoutePoint.getPosition().lng());
                        northEast = new google.maps.LatLng(startOptimalRoutePoint.getPosition().lat(), finalOptimalRoutePoint.getPosition().lng());
                    }
                }
                /*if (canRealTimeRecounting) {
                    countWay(false);
                }*/
                //else {
                /*var bounds = new google.maps.LatLngBounds(startOptimalRoutePoint.getPosition(), finalOptimalRoutePoint.getPosition());
                map.fitBounds(bounds);*/
                var today = new Date();
                var h = today.getHours();
                var m = today.getMinutes();
                var strH;
                if (h < 10) strH = "0" + h.toString();
                else strH = h.toString();
                var strM;
                if (m < 10) strM = "0" + m.toString();
                else strM = m.toString();
                var timeStr = strH + ":" + strM;
                //alert(timeStr);
                $("input[name=time]").val(timeStr);
                //alert($("input[name=time]").val().toString());
                $("#start-final_points").css("display", "inherit");
                $("#start_route").css("display", "inherit");
                $("#results").css("display", "none");
                $("#customization").css("display", "none");
                $('#viev_menu_link').click();

                if (widthMaximized == true) {
                    /*$("#map").width('100%');
                    $("#map").width(($("#map").outerWidth(true) - $("#menu").outerWidth(true)).toString());//!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    $("#header").width('100%');
                    $("#header").width(($("#header").outerWidth(false) - $("#menu").outerWidth(true)).toString());*/
                    widthMaximized = false;
                }
                //countOptimalRoute(startOptimalRoutePoint.getPosition(), finalOptimalRoutePoint.getPosition());

                var bounds = new google.maps.LatLngBounds(southWest, northEast);
                map.fitBounds(bounds);
                map.setCenter(new google.maps.LatLng((southWest.lat() + northEast.lat()) / 2, (southWest.lng() + northEast.lng()) / 2));
                //map.setZoom(map.getZoom() - 1);
                //}
                
            }
            else
            {
                $("#start_route").css("display", "none");
            }
        }
        function countWay(/*needViewAdvancedMenu*/)
        {
            $("#start-final_points").css("display", "none");
            $("#start_route").css("display", "none"); 
            $("#waiting_route").css("display", "inherit");
            var types = new Array();
            $("input:checkbox[name=transportType]:checked").each(function () {
                types.push($(this).val());
            });
            var r = getOptimalWays(startOptimalRoutePoint.getPosition(), finalOptimalRoutePoint.getPosition(), types, $("input[name=time]").val().toString(), $("input[name=reservedTime]").val().toString(), $("input[name=goingSpeed]").val().toString());
            r.done(function (res33) {
                findedOptimalWays = JSON.parse(res33.toString());
                $("#waiting_route").css("display", "none");
                $("#results").css("display", "inherit");
                $("#customization").css("display", "inherit");
                customizeFindedOptimalWays(/*totalTimePercent, totalGoingTimePercent, totalTransportChangingCountPercent*/);
                //$("#results_only").height(/*($("body").height() - $("#only_customization").outerHeight(true)).toString()*/"200");
                /*if (needViewAdvancedMenu != false) */$('#viev_menu_link').click();
            });
        }

        function setStartOptimalRoutePoint(currentLatLng) {
            $("#start_route").css("display", "none");
            if (startOptimalRoutePoint != null) startOptimalRoutePoint.setMap(null);
            if (currentLatLng != null) {
                startOptimalRoutePoint = new google.maps.Marker({
                    position: currentLatLng,
                    title: 'Начальная точка маршрута',
                    label: 'A',
                    draggable: true
                });
                startOptimalRoutePoint.addListener('dragend', function () { tryCountOptimalRoute(); });
                startOptimalRoutePoint.setMap(map);
                map.setCenter(currentLatLng);
                tryCountOptimalRoute();
            }
        }
        function setFinalOptimalRoutePoint(currentLatLng) {
            $("#start_route").css("display", "none");
            if (finalOptimalRoutePoint != null) finalOptimalRoutePoint.setMap(null);
            if (currentLatLng != null) {
                finalOptimalRoutePoint = new google.maps.Marker({
                    position: currentLatLng,
                    title: 'Конечная точка маршрута',
                    label: 'B',
                    draggable: true
                });
                finalOptimalRoutePoint.addListener('dragend', function () { tryCountOptimalRoute(); });
                finalOptimalRoutePoint.setMap(map);
                map.setCenter(currentLatLng);
                tryCountOptimalRoute();
            }
        }




        function getOptimalWays(from, to, types, time, reservedTime, goingSpeed) {
            $("#start_route").css("display", "none");
            $("#results").css("display", "none");
            $("#customization").css("display", "none");
            var deferred = new $.Deferred();
            var myCrdsFrom = from.lat().toFixed(4) + ',' + from.lng().toFixed(4);
            var myCrdsTo = to.lat().toFixed(4) + ',' + to.lng().toFixed(4);
            //alert(reservedTime);
            var reqStr = apiPublicTransportServer + "OptimalRoute?from=" + myCrdsFrom + "&to=" + myCrdsTo + "&startTime=" + time + "&dopTimeMinutes=" + reservedTime + "&goingSpeed=" + goingSpeed + "&transportTypes=";
            if (types != null && types.length != 0) reqStr += types;
            else reqStr += "null";
            var xhr22 = xhrGet(reqStr);
            xhr22.done(function (res222) {
                //alert(res2);
                //return res2;
                deferred.resolve(res222);
            });
            return deferred;
        }











        function addFindedWayToList(currentWay, minimalTimeSeconds, minimalGoingTimeSeconds, minimalTransportChangingCount)
        {
            var result_points = new Array();
            for (var i = 0; i < currentWay.points.length; i++) {
                var nw = new google.maps.LatLng(
                    currentWay.points[i].coords.xCoord,
                    currentWay.points[i].coords.yCoord
                );
                result_points.push(nw);
            }

            $('#customization_result').append(jQuery('<div/>'));
            var myDiv = $('#customization_result div:last-child');

            var tmpTotalTimeSecondsEffictivity = (minimalTimeSeconds / parseFloat(currentWay.totalTimeSeconds) * 100).toFixed(0);
            var tmpTotalGoingTimeSecondsEffictivity = (minimalGoingTimeSeconds / parseFloat(currentWay.totalGoingTimeSeconds) * 100).toFixed(0);
            var tmpTransportChangingCountEffictivity = (parseFloat(currentWay.totalTransportChangingCount) == 0 ? 1 : (minimalTransportChangingCount / parseFloat(currentWay.totalTransportChangingCount)) * 100).toFixed(0);
            
            var p1 = jQuery('<span/>', { text: tmpTotalTimeSecondsEffictivity.toString() + "%", title: 'TotalTimeSecondsEffictivity' });
            var p2 = jQuery('<span/>', { text: tmpTotalGoingTimeSecondsEffictivity.toString() + "%", title: 'TotalGoingTimeSecondsEffictivity' });
            var p3 = jQuery('<span/>', { text: tmpTransportChangingCountEffictivity.toString() + "%", title: 'TransportChangingCountEffictivity' });

            if (tmpTotalTimeSecondsEffictivity > 85) p1.addClass("effectivityPercentGold");
            else if (tmpTotalTimeSecondsEffictivity > 50) p1.addClass("effectivityPercentPlatinum");
            else p1.addClass("effectivityPercentSilver");

            if (tmpTotalGoingTimeSecondsEffictivity > 85) p2.addClass("effectivityPercentGold");
            else if (tmpTotalGoingTimeSecondsEffictivity > 50) p2.addClass("effectivityPercentPlatinum");
            else p2.addClass("effectivityPercentSilver");

            if (tmpTransportChangingCountEffictivity > 85) p3.addClass("effectivityPercentGold");
            else if (tmpTransportChangingCountEffictivity > 50) p3.addClass("effectivityPercentPlatinum");
            else p3.addClass("effectivityPercentSilver");

            myDiv.append(p1);
            myDiv.append(p2);
            myDiv.append(p3);

            myDiv.addClass("resultLink");
            myDiv.click(function () {
                /*var result_points = new Array();
                for (var i = 0; i < currentWay.points.length; i++) {
                    var nw = new google.maps.LatLng(
                        currentWay.points[i].coords.xCoord,
                        currentWay.points[i].coords.yCoord
                    );
                    result_points.push(nw);
                }*/
                if (wayViewLine != null) wayViewLine.setMap(null);
                wayViewLine = new google.maps.Polyline({
                    path: result_points,
                    geodesic: true,
                    strokeColor: '#FF0000',//#FCD6A4
                    strokeOpacity: 1.0,
                    strokeWeight: 5
                });
                wayViewLine.setMap(map);
                wayViewLine.addListener('click', function () { flightPath.setMap(null); });
                //alert("sending data...");
            });
            //alert('saddsfd');
            for (var i = 1; i < currentWay.points.length; i++) {
                var my_text = "";
                if (currentWay.points[i].route == null) {
                    if (currentWay.points[i].station == null) my_text = "Идите пешком к пункту назначения.";
                    else {
                        my_text = "Идите к остановке \"";
                        if (currentWay.points[i].station.nameRus.toString() != "") my_text += currentWay.points[i].station.nameRus.toString();
                        else my_text += currentWay.points[i].station.name.toString();
                        my_text += "\"";
                    }
                }
                else {
                    if (i + 1 < currentWay.points.length && currentWay.points[i + 1].route != null && currentWay.points[i].route.type == currentWay.points[i + 1].route.type && currentWay.points[i].route.number == currentWay.points[i + 1].route.number) continue;
                    my_text = "Доедьте до остановки \"";
                    if (currentWay.points[i].station.nameRus.toString() != "") my_text += currentWay.points[i].station.nameRus.toString();
                    else my_text += currentWay.points[i].station.name.toString();
                    my_text += "\" на транспорте \"" + currentWay.points[i].route.type.toString() + " " + currentWay.points[i].route.number.toString() + "\"";
                }
                /*var hours = Math.round(currentWay.totalTimeSeconds / 3600);
                var minutes = Math.round((currentWay.totalTimeSeconds % 3600) / 60);
                var seconds = (currentWay.totalTimeSeconds % 3600) % 60;
                my_text += " (" + hours.toString() + ":" + minutes.toString() + ":" + seconds.toString() + ")";*/
                my_text += " (" + currentWay.points[i].time + ")";
                myDiv.append(
                    jQuery('<li/>').append(
                        jQuery('<span/>', {
                            css: {
                                cursor: 'pointer',
                            },
                            text: my_text,
                            /*click: function () {
                                //alert("sending data...");
                                var xhr2 = xhrPut(apiPublicTransportServer + "stations/S157A8E088DD", "MY TEXT 123 !%^&*");
                                xhr2.done(function (res2) {
                                    //alert(res2.toString());
                                    $('#test_t').append(jQuery('<span/>', { text: res2 }));
                                });
                            }*/
                        })
                    )
                );
                /*var nw = new google.maps.LatLng(
                    currentWay.points[i].coords.xCoord,
                    currentWay.points[i].coords.yCoord
                );
                var tmpMarker = new google.maps.Marker({
                    position: nw,
                    map: map,
                    //icon: image,
                    title: my_text
                });
                tmpMarker.setMap(map);*/
            }
            if (wayViewLine != null) wayViewLine.setMap(null);
            wayViewLine = new google.maps.Polyline({
                path: result_points,
                geodesic: true,
                strokeColor: '#FF0000',//#FCD6A4
                strokeOpacity: 1.0,
                strokeWeight: 5
            });
            wayViewLine.setMap(map);
            wayViewLine.addListener('click', function () { flightPath.setMap(null); });
            //alert("Recounted: "+totalTimePercent.toString()+", "+totalGoingTimePercent.toString()+", "+totalTransportChangingCountPercent.toString());
        }

        var wayViewLine = null;
        function customizeFindedOptimalWays(/*totalTimePercent, totalGoingTimePercent, totalTransportChangingCountPercent*/)
        {
            if (findedOptimalWays != null)
            {
                totalTimePercent = $("input[name=totalTimePercent]").val();
                totalGoingTimePercent = $("input[name=totalGoingTimePercent]").val();
                totalTransportChangingCountPercent = $("input[name=totalTransportChangingCountPercent]").val();

                var minimalTimeSeconds = parseFloat(findedOptimalWays[0].totalTimeSeconds);
                var minimalGoingTimeSeconds = parseFloat(findedOptimalWays[0].totalGoingTimeSeconds);
                var minimalTransportChangingCount = parseFloat(findedOptimalWays[0].totalTransportChangingCount);
                for (var i = 1; i < findedOptimalWays.length; i++)
                {
                    if (parseFloat(findedOptimalWays[i].totalTimeSeconds) < minimalTimeSeconds) minimalTimeSeconds = parseFloat(findedOptimalWays[i].totalTimeSeconds);
                    if (parseFloat(findedOptimalWays[i].totalGoingTimeSeconds) < minimalGoingTimeSeconds) minimalGoingTimeSeconds = parseFloat(findedOptimalWays[i].totalGoingTimeSeconds);
                    if (parseFloat(findedOptimalWays[i].totalTransportChangingCount) < minimalTransportChangingCount) minimalTransportChangingCount = parseFloat(findedOptimalWays[i].totalTransportChangingCount);
                }
                if (minimalTransportChangingCount < 1) minimalTransportChangingCount = 1;
                /*if (minimalTimeSeconds < 1) minimalTimeSeconds = 1;
                if (minimalGoingTimeSeconds < 1) minimalGoingTimeSeconds = 1;*/
                //alert(minimalTimeSeconds.toString() + ", " + minimalGoingTimeSeconds.toString() + ", " + minimalTransportChangingCount.toString());

                var sortedArr = new Array();

                $('#customization_result').text("");

                var tmpTransportChangingCountEffictivity = 0;//parseFloat(findedOptimalWays[0].totalTransportChangingCount) == 0 ? 1 : (minimalTransportChangingCount / parseFloat(findedOptimalWays[0].totalTransportChangingCount));
                var max_rank = 0;//minimalTimeSeconds / parseFloat(findedOptimalWays[0].totalTimeSeconds) * totalTimePercent + minimalGoingTimeSeconds / parseFloat(findedOptimalWays[0].totalGoingTimeSeconds) * totalGoingTimePercent + tmpTransportChangingCountEffictivity * totalTransportChangingCountPercent;
                var index = -1;
                for (var j = 0; j < findedOptimalWays.length/* && j < 3*/; j++) {
                    index = -1;
                    for (var i = 0; i < findedOptimalWays.length; i++) {
                        //alert((jQuery.inArray(i, sortedArr)).toString());
                        if (jQuery.inArray(i, sortedArr) == -1) {
                            tmpTransportChangingCountEffictivity = parseFloat(findedOptimalWays[i].totalTransportChangingCount) == 0 ? 1 : (minimalTransportChangingCount / parseFloat(findedOptimalWays[i].totalTransportChangingCount));
                            var tmp_rank = minimalTimeSeconds / parseFloat(findedOptimalWays[i].totalTimeSeconds) * totalTimePercent + minimalGoingTimeSeconds / parseFloat(findedOptimalWays[i].totalGoingTimeSeconds) * totalGoingTimePercent + tmpTransportChangingCountEffictivity * totalTransportChangingCountPercent;
                            if (tmp_rank > max_rank) {
                                max_rank = tmp_rank;
                                index = i;
                            }
                        }
                        //else alert(i.toString());
                    }
                    if (index != -1) {
                        sortedArr.push(index);
                        //alert(index.toString());
                        //addFindedWayToList(findedOptimalWays[index]);
                    }
                    addFindedWayToList(findedOptimalWays[j], minimalTimeSeconds, minimalGoingTimeSeconds, minimalTransportChangingCount);
                }
                //alert(sortedArr.length.toString());

                
                //addFindedWayToList(findedOptimalWays, index);
                //addFindedWayToList(findedOptimalWays, index);
                //addFindedWayToList(findedOptimalWays, index);
            }
        }





        function showContextMenu(caurrentLatLng) {
            //alert(caurrentLatLng);
            var projection;
            var contextmenuDir;
            projection = map.getProjection();
            $('.contextmenu').remove();
            contextmenuDir = document.createElement("div");
            contextmenuDir.className = 'contextmenu';
            /*contextmenuDir.innerHTML = '<a id="menu1" onclick="$(\'.contextmenu\').remove();" style="cursor:pointer;"><div class="context">Проложить маршрут отсюда<\/div><\/a>'
                                    + '<a id="menu2" onclick="$(\'.contextmenu\').remove();" style="cursor:pointer;"><div class="context">Проложить маршрут сюда<\/div><\/a>';*/
            var link_a = document.createElement("a");
            link_a.className = 'contextLink';
            link_a.innerHTML = '<div class="context">Проложить маршрут отсюда<\/div>';
            link_a.onclick = function () { setStartOptimalRoutePoint(caurrentLatLng); $('.contextmenu').remove(); };

            var link_b = document.createElement("a");
            link_b.className = 'contextLink';
            link_b.innerHTML = '<div class="context">Проложить маршрут сюда<\/div>';
            link_b.onclick = function () { setFinalOptimalRoutePoint(caurrentLatLng); $('.contextmenu').remove(); };
            

            contextmenuDir.appendChild(link_a);
            contextmenuDir.appendChild(link_b);
            //contextmenuDir = $('.contextmenu');


            $(map.getDiv()).append(contextmenuDir);

            setMenuXY(caurrentLatLng);

            contextmenuDir.style.visibility = "visible";
        }

        function getCanvasXY(caurrentLatLng) {
            var scale = Math.pow(2, map.getZoom());
            var nw = new google.maps.LatLng(
                map.getBounds().getNorthEast().lat(),
                map.getBounds().getSouthWest().lng()
            );
            var worldCoordinateNW = map.getProjection().fromLatLngToPoint(nw);
            var worldCoordinate = map.getProjection().fromLatLngToPoint(caurrentLatLng);
            var caurrentLatLngOffset = new google.maps.Point(
                Math.floor((worldCoordinate.x - worldCoordinateNW.x) * scale),
                Math.floor((worldCoordinate.y - worldCoordinateNW.y) * scale)
            );
            return caurrentLatLngOffset;
        }

        function setMenuXY(caurrentLatLng) {
            var mapWidth = $('#map_canvas').width();
            var mapHeight = $('#map_canvas').height();
            var menuWidth = $('.contextmenu').width();
            var menuHeight = $('.contextmenu').height();
            var clickedPosition = getCanvasXY(caurrentLatLng);
            var x = clickedPosition.x;
            var y = clickedPosition.y;

            if ((mapWidth - x) > menuWidth)//if to close to the map border, decrease x position
                x = x - menuWidth;
            if ((mapHeight - y) > menuHeight)//if to close to the map border, decrease y position
                y = y - menuHeight;

            $('.contextmenu').css('left', x);
            $('.contextmenu').css('top', y);
        };



        function getPointCoordsFromGoogleGeocodingApi(strReq) {
            var deferred = new $.Deferred();
            var xhr = new XMLHttpRequest();
            var centerCoords = map.getCenter();
            var cLat = centerCoords.lat().toFixed(6).toString().replace(',', '.');
            var cLng = centerCoords.lng().toFixed(6).toString().replace(',', '.');
            xhr.open("GET", "https://maps.googleapis.com/maps/api/geocode/json?address=" + strReq + "&bounds=" + cLat + "," + cLng + "|" + cLat + "," + cLng, true);
            xhr.onreadystatechange = function () {
                if (xhr.readyState != 4) return;
                if (xhr.status == 200) {
                    deferred.resolve(xhr.responseText);
                } else {
                    //deferred.errback(xhr.statusText);
                }
            }
            xhr.send(null);
            return deferred;
        }
        //http://nominatim.openstreetmap.org/search?q=гродно+ожешко+университет&format=json
        function getPointCoordsFromOsmGeocodingApi(strReq) {
            var deferred = new $.Deferred();
            var xhr = new XMLHttpRequest();
            xhr.open("GET", "http://nominatim.openstreetmap.org/search?q=" + strReq + "&format=json", true);
            xhr.onreadystatechange = function () {
                if (xhr.readyState != 4) return;
                if (xhr.status == 200) {
                    deferred.resolve(xhr.responseText);
                } else {
                    //deferred.errback(xhr.statusText);
                }
            }
            xhr.send(null);
            return deferred;
        }
        function findPoint(inputName, type)
        {
            var strReq = $("input[name=" + inputName + "]").val().toString();
            //var tmp = getPointCoordsFromGoogleGeocodingApi(strReq);
            var tmp = getPointCoordsFromOsmGeocodingApi(strReq);
            return tmp.done(function (returnedPointsJSON) {
                var allPoints = JSON.parse(returnedPointsJSON.toString());
                //alert(returnedPointsJSON.toString());
                if (allPoints != null && allPoints.length != 0 && (type == "start" || type == "final")) {
                    
                    var coords = {
                        lat: parseFloat(allPoints[0].lat),
                        lng: parseFloat(allPoints[0].lon)
                    };
                    //var findedName = allPoints[0].display_name;//results[0].formatted_address;
                    //$("input[name=" + inputName + "]").val(findedName);
                    if (type == "start") setStartOptimalRoutePoint(coords);
                    else setFinalOptimalRoutePoint(coords);
                }
                else
                {
                    //alert('asddsgfgs');
                    if (type == "start") {
                        if (startOptimalRoutePoint!=null) startOptimalRoutePoint.setMap(null);
                        startOptimalRoutePoint = null;
                    }
                    else if (type == "final") {
                        if(finalOptimalRoutePoint!=null)finalOptimalRoutePoint.setMap(null);
                        finalOptimalRoutePoint = null;
                    }
                    tryCountOptimalRoute();
                    alert("К сожалению, не удалось что-то найти по вашему запросу.");
                    //$("input[name=" + inputName + "]").val('');
                    //return null;
                }
            });
        }
    </script>
    <script src="https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/markerclusterer.js"></script>
    <script type="text/javascript">
			$(function() {
				$('nav#menu').mmenu({
					extensions				: [ 'effect-slide-menu', 'shadow-page', 'shadow-panels' ],
					keyboardNavigation 		: true,
					screenReader 			: true,
					counters				: true,
					navbar 	: {
						title	: 'Advanced menu'
					},
					/*navbars	: [
						{
							position	: 'top',
							content		: [ 'searchfield' ]
						}, {
							position	: 'top',
							content		: [
								'prev',
								'title',
								'close'
							]
						}, {
						    position: 'bottom',
						    content: [
								'<a href="http://mmenu.frebsite.nl/wordpress-plugin.html" target="_blank">WordPress plugin</a>'
						    ]
						}
					]*/
				});
			});


			
    </script>
</head>
<body onresize="javascript: $('#map').height('100%'); $('#map').height(($('#map').outerHeight(true) - $('.header').outerHeight(true)).toString());" onload="initialize()">
    
    <div id="page">
        <div onclick="" id="header" class="header">
            <a id="viev_menu_link" href="#menu"><span></span></a>
            Public Transport Project
        </div>
        <div onclick="" class="content">
            <div id="map">Loading map...</div>
            <div class="contextmenu"></div>
        </div>
        <nav id="menu">
            <div id="my_elements">
                <div id="start-final_points">
                    <p>Сведения о прокладываемом маршруте:</p>
                    <div>
                        <form action="#">
                           
                            <label>Начальная точка: <input name="startPointSearch" onchange="" type="text"><input type="button" value="Найти" onclick="findPoint('startPointSearch', 'start');"></label>
                            <label>Конечная точка: <input name="finalPointSearch" onchange="" type="text"><input type="button" value="Найти" onclick="findPoint('finalPointSearch', 'final');"></label>
                        </form>
                    </div>
                </div>
                <div id="start_route">
                    <p>Параметры построения маршрута:</p>
                    <div>
                        <form action="#">
                            <label>
                                Виды транспорта:
                                <label class="checkbox_elem"><input name="transportType" checked onchange="" type="checkbox" value="bus">Автобус</label>
                                <label class="checkbox_elem"><input name="transportType" checked onchange="" type="checkbox" value="trolleybus">Троллейбус</label>
                                <label class="checkbox_elem"><input name="transportType" onchange="" type="checkbox" value="express_bus">Экспресс-автобус</label>
                                <label class="checkbox_elem"><input name="transportType" disabled onchange="" type="checkbox" value="marsh">Маршрутка</label>
                                <label class="checkbox_elem"><input name="transportType" disabled onchange="" type="checkbox" value="tram">Трамвай</label>
                                <label class="checkbox_elem"><input name="transportType" disabled onchange="" type="checkbox" value="metro">Метро</label>
                            </label>
                            <label>Время отправки: <input name="time" type="time" value="12:00"></label>
                            <label class="block_elem">Going speed: <input name="goingSpeed" onchange="" type="range" min="2" max="10" step="0.5" value="5"></label>
                            <label class="block_elem">Reserved time: <input name="reservedTime" onchange="" type="range" min="0" max="5" step="0.5" value="2"></label>
                            <input class="checkbox_elem" type="button" value="Проложить маршрут" onclick="countWay();">
                        </form>
                    </div>
                </div>
                <div id="waiting_route">
                    <p>Идет поиск маршрута. Подождите немного...</p>
                </div>
                    <div id="customization">
                        <form action="#"><input class="block_elem" type="button" value="Назад" onclick="tryCountOptimalRoute();"></form>
                        <p>Customization:</p>
                        <div>
                            <form action="#">
                                <label>
                                    Уровни значимости по критериям:
                                    <label class="block_elem"><input name="totalTimePercent" onmousemove="customizeFindedOptimalWays();" onchange="customizeFindedOptimalWays();" type="range" min="0" max="1" step="0.05" value="1">Минимум времени</label>
                                    <label class="block_elem"><input name="totalGoingTimePercent" onmousemove="customizeFindedOptimalWays();" onchange="customizeFindedOptimalWays();" type="range" min="0" max="1" step="0.05" value="0.5">Минимум ходьбы</label>
                                    <label class="block_elem"><input name="totalTransportChangingCountPercent" onmousemove="customizeFindedOptimalWays();" onchange="customizeFindedOptimalWays();" type="range" min="0" max="1" step="0.05" value="0.05">Минимум пересадок</label>
                                </label>
                            </form>
                        </div>
                    </div>
                    <div id="results">
                        <p>Маршрут:</p>
                        <span id="customization_result">(...result...)</span>
                    </div>
                
            </div>
            
        </nav>
    </div>

    
    
    
    
</body>
</html>

