﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.Engine.CORE;
using Lampac.Models.LITE;
using Newtonsoft.Json.Linq;
using Lampac.Engine.CORE;
using System.Net;
using Shared.Model.Templates;
using System;

namespace Lampac.Controllers.LITE
{
    public class RgShows : BaseENGController
    {
        [HttpGet]
        [Route("lite/rgshows")]
        public Task<ActionResult> Index(bool checksearch, long id, string imdb_id, string title, string original_title, int serial, int s = -1, bool rjson = false)
        {
            return ViewTmdb(AppInit.conf.Rgshows, false, checksearch, id, imdb_id, title, original_title, serial, s, rjson, mp4: true, method: "call", hls_manifest_timeout: (int)TimeSpan.FromSeconds(30).TotalMilliseconds);
        }


        #region Video
        [HttpGet]
        [Route("lite/rgshows/video")]
        async public Task<ActionResult> Video(string imdb_id, int s = -1, int e = -1, bool play = false)
        {
            var init = await loadKit(AppInit.conf.Rgshows);
            if (await IsBadInitialization(init, rch: false))
                return badInitMsg;

            if (string.IsNullOrEmpty(imdb_id))
                return OnError();

            var proxyManager = new ProxyManager(init);
            var proxy = proxyManager.BaseGet();

            string embed = $"{init.host}/main/movie/{imdb_id}";
            if (s > 0)
                embed = $"{init.host}/main/tv/{imdb_id}/{s}/{e}";

            string file = await magic(embed, init, proxy.proxy);
            if (file == null)
                return StatusCode(502);

            file = HostStreamProxy(init, file, proxy: proxy.proxy);

            if (play)
                return Redirect(file);

            return ContentTo(VideoTpl.ToJson("play", file, "English", vast: init.vast));
        }
        #endregion

        #region magic
        async ValueTask<string> magic(string uri, OnlinesSettings init, WebProxy proxy)
        {
            if (string.IsNullOrEmpty(uri))
                return uri;

            try
            {
                string memKey = $"rgshows:{uri}";
                if (!hybridCache.TryGetValue(memKey, out string file))
                {
                    var root = await HttpClient.Get<JObject>(uri, timeoutSeconds: 40, headers: httpHeaders(init));
                    if (root == null || !root.ContainsKey("stream"))
                        return null;

                    file = root["stream"].Value<string>("url");
                    if (string.IsNullOrEmpty(file))
                        return null;

                    hybridCache.Set(memKey, file, cacheTime(20, init: init));
                }

                return file;
            }
            catch { return null; }
        }
        #endregion
    }
}
