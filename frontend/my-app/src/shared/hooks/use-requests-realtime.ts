"use client";

import { useEffect, useRef, useState } from "react";
import {
  HubConnectionBuilder,
  LogLevel,
  HttpTransportType,
  HubConnection,
} from "@microsoft/signalr";
import { apiRoutes } from "@/shared/constants/api-routes";
import type { RequestDto } from "@/shared/types/requests";

export type RequestRealtimeEventCallbacks = {
  onRequestCreated?: (request: RequestDto) => void;
  onRequestUpdated?: (request: RequestDto) => void;
  onRequestDeleted?: (requestId: string) => void;
};

export function useRequestsRealtime(callbacks: RequestRealtimeEventCallbacks) {
  const callbacksRef = useRef(callbacks);
  const connectionRef = useRef<HubConnection | null>(null);

  const [connectionStatus, setConnectionStatus] = useState<
    "connected" | "connecting" | "reconnecting" | "disconnected"
  >("connecting");

  useEffect(() => {
    callbacksRef.current = callbacks;
  }, [callbacks]);

  useEffect(() => {
    // 1. Build the connection and save it to a ref
    const connection = new HubConnectionBuilder()
      .withUrl(apiRoutes.requestsHub, {
        withCredentials: true,
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect([0, 2000, 10000])
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    // 2. Set up event listeners
    connection.on("RequestCreated", (request: RequestDto) => {
      callbacksRef.current.onRequestCreated?.(request);
    });

    connection.on("RequestUpdated", (request: RequestDto) => {
      callbacksRef.current.onRequestUpdated?.(request);
    });

    connection.on("RequestDeleted", (requestId: string) => {
      callbacksRef.current.onRequestDeleted?.(requestId);
    });

    connection.onreconnecting(() => setConnectionStatus("reconnecting"));
    connection.onreconnected(() => setConnectionStatus("connected"));
    connection.onclose(() => setConnectionStatus("disconnected"));

    // 3. Track starting state to prevent race conditions during cleanup
    let isMounted = true;
    setConnectionStatus("connecting");

    connection
      .start()
      .then(() => {
        if (isMounted) {
          setConnectionStatus("connected");
        } else {
          // If the component unmounted while starting, stop it now safely
          void connection.stop();
        }
      })
      .catch(() => {
        if (isMounted) setConnectionStatus("disconnected");
      });

    // 4. Cleanup function
    return () => {
      isMounted = false;

      connection.off("RequestCreated");
      connection.off("RequestUpdated");
      connection.off("RequestDeleted");

      // Only call stop if it's fully connected, otherwise the .then() block above handles it
      if (connection.state === "Connected") {
        void connection.stop();
      }

      connectionRef.current = null;
    };
  }, []);

  return { connectionStatus };
}
