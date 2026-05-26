import { NextRequest, NextResponse } from "next/server";
import { apiRoutes } from "./shared/constants/api-routes";
import { appRoutes } from "./shared/constants/app-routes";

async function refreshAuth(req: NextRequest) {
  const refreshUrl =
    apiRoutes.refresh ||
    new URL("/api/account/refresh", req.nextUrl.origin).toString();
  const cookieHeader = req.headers.get("cookie") ?? "";

  const refreshResponse = await fetch(refreshUrl, {
    method: "POST",
    headers: cookieHeader ? { cookie: cookieHeader } : undefined,
    credentials: "include",
  });

  if (!refreshResponse.ok) {
    return null;
  }

  const response = NextResponse.next();
  for (const [key, value] of refreshResponse.headers) {
    if (key.toLowerCase() === "set-cookie") {
      response.headers.append("set-cookie", value);
    }
  }

  return response;
}

export async function proxy(req: NextRequest) {
  const pathname = req.nextUrl.pathname;
  const accessToken = req.cookies.get("ACCESS_TOKEN")?.value;
  const refreshToken = req.cookies.get("REFRESH_TOKEN")?.value;
  const loginUrl = new URL(appRoutes.login, req.url);
  const homeUrl = new URL(appRoutes.home, req.url);

  if (!accessToken && refreshToken) {
    const refreshed = await refreshAuth(req);
    if (refreshed) {
      if (pathname === "/login") {
        const redirectResponse = NextResponse.redirect(homeUrl);
        for (const [key, value] of refreshed.headers) {
          if (key.toLowerCase() === "set-cookie") {
            redirectResponse.headers.append("set-cookie", value);
          }
        }
        return redirectResponse;
      }
      return refreshed;
    }
  }

  if (!accessToken) {
    if (pathname === "/login") {
      return NextResponse.next();
    }
    return NextResponse.redirect(loginUrl);
  }

  if (pathname === "/login") {
    return NextResponse.redirect(homeUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    // Skip API, Next.js internals and all static files, unless found in search params
    "/((?!api|_next|[^?]*\\.(?:html?|css|js(?!on)|jpe?g|webp|png|gif|svg|ttf|woff2?|ico|csv|docx?|xlsx?|zip|webmanifest)).*)",
    // Always run for API routes
    //"/(api|trpc)(.*)",
  ],
};
