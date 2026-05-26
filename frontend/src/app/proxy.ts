import { NextRequest } from "next/server";

export function proxy(req: NextRequest) {}

export const config = {
  matcher: [
    // Skip API, Next.js internals and all static files, unless found in search params
    "/((?!api|_next|[^?]*\\.(?:html?|css|js(?!on)|jpe?g|webp|png|gif|svg|ttf|woff2?|ico|csv|docx?|xlsx?|zip|webmanifest)).*)",
    // Always run for API routes
    //"/(api|trpc)(.*)",
  ],
};
