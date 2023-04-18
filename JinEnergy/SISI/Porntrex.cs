﻿using System.Web;
using JinEnergy.Engine;
using Microsoft.JSInterop;
using Shared.Engine.SISI;

namespace JinEnergy.SISI
{
    public class PorntrexController : BaseController
    {
        [JSInvokable("ptx")]
        async public static Task<dynamic> Index(string args)
        {
            string? search = arg("search", args);
            string? sort = arg("sort", args);
            int pg = int.Parse(arg("pg", args) ?? "1");

            string? html = await PorntrexTo.InvokeHtml(AppInit.Porntrex.corsHost(), search, sort, pg, url => JsHttpClient.Get(url));
            if (html == null)
                return OnError("html");

            return new
            {
                menu = PorntrexTo.Menu(null, sort),
                list = PorntrexTo.Playlist("ptx/vidosik", html, picture => $"http://vi.sisi.am/poster.jpg?href={HttpUtility.UrlEncode(picture)}&r=200")
            };
        }


        [JSInvokable("ptx/vidosik")]
        async public static Task<dynamic> Stream(string args)
        {
            var stream_links = await PorntrexTo.StreamLinks(AppInit.Porntrex.corsHost(), arg("uri", args), url => JsHttpClient.Get(url));
            if (stream_links == null)
                return OnError("stream_links");

            return stream_links;
        }
    }
}
