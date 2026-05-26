"use client";

import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import { Loader2, RefreshCw, Trash2 } from "lucide-react";
import { apiRoutes } from "@/shared/constants/api-routes";
import { useRequestsRealtime } from "@/shared/hooks/use-requests-realtime";
import { Button } from "@/shared/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import type {
  CreateRequestRequest,
  RequestDto,
  RequestStatus,
  UpdateRequestStatusRequest,
} from "@/shared/types/requests";

const requestStatusOptions = [
  { label: "New", value: 0 as RequestStatus },
  { label: "In progress", value: 1 as RequestStatus },
  { label: "Completed", value: 2 as RequestStatus },
  { label: "Cancelled", value: 3 as RequestStatus },
];

const statusLabelMap: Record<RequestStatus, string> = {
  0: "New",
  1: "In progress",
  2: "Completed",
  3: "Cancelled",
};

const buildQueryString = (params: Record<string, string | undefined>) => {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value) {
      searchParams.set(key, value);
    }
  });

  const query = searchParams.toString();
  return query ? `?${query}` : "";
};

export function RequestsDashboard() {
  const [requests, setRequests] = useState<RequestDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ clientName: "", phone: "", comment: "" });
  const [filter, setFilter] = useState({ clientName: "", status: "" });
  const [activeRequestId, setActiveRequestId] = useState<string | null>(null);

  const statusOptions = useMemo(
    () => [{ label: "All", value: "" }, ...requestStatusOptions],
    [],
  );

  const isRequestVisible = useCallback(
    (request: RequestDto) => {
      const clientNameMatches =
        !filter.clientName ||
        request.clientName
          .toLowerCase()
          .includes(filter.clientName.toLowerCase());
      const statusMatches =
        !filter.status || String(request.status) === filter.status;

      return clientNameMatches && statusMatches;
    },
    [filter.clientName, filter.status],
  );

  const loadRequests = async () => {
    setLoading(true);
    setError(null);

    try {
      const queryString = buildQueryString({
        clientName: filter.clientName || undefined,
        status: filter.status || undefined,
      });

      const response = await fetch(`${apiRoutes.requests}${queryString}`, {
        method: "GET",
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Не удалось загрузить запросы.");
      }

      const data = (await response.json()) as RequestDto[];
      setRequests(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Произошла ошибка при загрузке.",
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadRequests();
  }, [filter.clientName, filter.status]);

  const handleRealtimeRequestCreated = useCallback(
    (request: RequestDto) => {
      if (!isRequestVisible(request)) {
        return;
      }

      setRequests((current) => {
        if (current.some((item) => item.id === request.id)) {
          return current;
        }

        return [request, ...current];
      });
    },
    [isRequestVisible],
  );

  const handleRealtimeRequestUpdated = useCallback(
    (request: RequestDto) => {
      setRequests((current) => {
        const exists = current.some((item) => item.id === request.id);
        if (exists) {
          return current.map((item) =>
            item.id === request.id ? request : item,
          );
        }

        return isRequestVisible(request) ? [request, ...current] : current;
      });
    },
    [isRequestVisible],
  );

  const handleRealtimeRequestDeleted = useCallback((requestId: string) => {
    setRequests((current) => current.filter((item) => item.id !== requestId));
  }, []);

  const { connectionStatus } = useRequestsRealtime({
    onRequestCreated: handleRealtimeRequestCreated,
    onRequestUpdated: handleRealtimeRequestUpdated,
    onRequestDeleted: handleRealtimeRequestDeleted,
  });

  const handleCreateRequest = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSaving(true);
    setError(null);

    const payload: CreateRequestRequest = {
      clientName: form.clientName.trim(),
      phone: form.phone.trim(),
      comment: form.comment.trim() || undefined,
    };

    if (!payload.clientName || !payload.phone) {
      setError("Пожалуйста, заполните имя клиента и телефон.");
      setSaving(false);
      return;
    }

    try {
      const response = await fetch(apiRoutes.requests, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        throw new Error("Не удалось создать запрос.");
      }

      setForm({ clientName: "", phone: "", comment: "" });
      await loadRequests();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Произошла ошибка при создании запроса.",
      );
    } finally {
      setSaving(false);
    }
  };

  const handleUpdateStatus = async (
    requestId: string,
    status: RequestStatus,
  ) => {
    setActiveRequestId(requestId);
    setError(null);

    const payload: UpdateRequestStatusRequest = { status };

    try {
      const response = await fetch(apiRoutes.updateRequestStatus(requestId), {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        throw new Error("Не удалось обновить статус запроса.");
      }

      const updated = (await response.json()) as RequestDto;
      setRequests((current) =>
        current.map((item) => (item.id === updated.id ? updated : item)),
      );
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Произошла ошибка при обновлении статуса.",
      );
    } finally {
      setActiveRequestId(null);
    }
  };

  const handleDeleteRequest = async (requestId: string) => {
    setActiveRequestId(requestId);
    setError(null);

    try {
      const response = await fetch(apiRoutes.deleteRequest(requestId), {
        method: "DELETE",
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Не удалось удалить запрос.");
      }

      setRequests((current) => current.filter((item) => item.id !== requestId));
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Произошла ошибка при удалении запроса.",
      );
    } finally {
      setActiveRequestId(null);
    }
  };

  return (
    <main className="min-h-screen bg-background px-4 py-8 text-foreground">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-6">
        <div className="grid gap-6 lg:grid-cols-[1.3fr_0.7fr]">
          <Card className="shadow-sm">
            <CardHeader className="gap-4">
              <div>
                <CardTitle>Requests dashboard</CardTitle>
                <p className="text-xs text-muted-foreground">
                  Realtime: {connectionStatus}
                </p>
              </div>
              <div className="flex flex-wrap items-center gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => void loadRequests()}
                >
                  <RefreshCw className="mr-2 h-4 w-4" />
                  Refresh
                </Button>
              </div>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid gap-2 sm:grid-cols-[1fr_180px]">
                <div>
                  <Label htmlFor="filter-client">Search by client</Label>
                  <Input
                    id="filter-client"
                    value={filter.clientName}
                    onChange={(event) =>
                      setFilter((current) => ({
                        ...current,
                        clientName: event.target.value,
                      }))
                    }
                    placeholder="Client name"
                  />
                </div>
                <div>
                  <Label htmlFor="filter-status">Status</Label>
                  <select
                    id="filter-status"
                    value={filter.status}
                    onChange={(event) =>
                      setFilter((current) => ({
                        ...current,
                        status: event.target.value,
                      }))
                    }
                    className="h-9 w-full rounded-md border border-input bg-transparent px-2.5 py-1 text-base shadow-xs outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
                  >
                    {statusOptions.map((option) => (
                      <option key={option.value} value={String(option.value)}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              {error ? (
                <div className="rounded-md border border-destructive/40 bg-destructive/10 p-4 text-sm text-destructive">
                  {error}
                </div>
              ) : null}
              <div className="rounded-xl border border-border bg-muted/50 p-4 text-sm text-muted-foreground">
                <p>
                  The list below shows requests from the API. Use the filters to
                  narrow the results and create new requests from the form on
                  the right.
                </p>
              </div>
            </CardContent>
          </Card>

          <Card className="shadow-sm">
            <CardHeader>
              <CardTitle>Create new request</CardTitle>
            </CardHeader>
            <CardContent>
              <form className="space-y-4" onSubmit={handleCreateRequest}>
                <div className="grid gap-2">
                  <Label htmlFor="clientName">Client name</Label>
                  <Input
                    id="clientName"
                    value={form.clientName}
                    onChange={(event) =>
                      setForm((current) => ({
                        ...current,
                        clientName: event.target.value,
                      }))
                    }
                    placeholder="Client name"
                  />
                </div>
                <div className="grid gap-2">
                  <Label htmlFor="phone">Phone</Label>
                  <Input
                    id="phone"
                    value={form.phone}
                    onChange={(event) =>
                      setForm((current) => ({
                        ...current,
                        phone: event.target.value,
                      }))
                    }
                    placeholder="Phone"
                  />
                </div>
                <div className="grid gap-2">
                  <Label htmlFor="comment">Comment</Label>
                  <Input
                    id="comment"
                    value={form.comment}
                    onChange={(event) =>
                      setForm((current) => ({
                        ...current,
                        comment: event.target.value,
                      }))
                    }
                    placeholder="Optional comment"
                  />
                </div>
                <Button type="submit" disabled={saving} className="w-full">
                  {saving ? (
                    <span className="flex items-center justify-center gap-2">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Saving...
                    </span>
                  ) : (
                    "Create request"
                  )}
                </Button>
              </form>
            </CardContent>
          </Card>
        </div>

        <section className="grid gap-4">
          {loading ? (
            <div className="flex items-center justify-center rounded-xl border border-border bg-background p-10 text-sm text-muted-foreground">
              <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Loading
              requests...
            </div>
          ) : requests.length === 0 ? (
            <div className="rounded-xl border border-dashed border-border/70 bg-background/50 p-10 text-center text-sm text-muted-foreground">
              No requests found.
            </div>
          ) : (
            <div className="grid gap-4">
              {requests.map((request) => (
                <Card key={request.id} className="shadow-sm">
                  <CardHeader className="items-start gap-2">
                    <div>
                      <CardTitle>{request.clientName}</CardTitle>
                      <p className="text-sm text-muted-foreground">
                        {request.phone}
                      </p>
                    </div>
                    <div className="rounded-full border border-border px-3 py-1 text-xs font-semibold uppercase tracking-[0.12em] text-muted-foreground">
                      {statusLabelMap[request.status]}
                    </div>
                  </CardHeader>
                  <CardContent className="grid gap-4">
                    <div className="grid gap-2 sm:grid-cols-[1fr_1fr]">
                      <div>
                        <Label htmlFor={`status-${request.id}`}>Status</Label>
                        <select
                          id={`status-${request.id}`}
                          value={String(request.status)}
                          onChange={(event) =>
                            void handleUpdateStatus(
                              request.id,
                              Number(event.target.value) as RequestStatus,
                            )
                          }
                          className="h-9 w-full rounded-md border border-input bg-transparent px-2.5 py-1 text-base shadow-xs outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
                        >
                          {requestStatusOptions.map((option) => (
                            <option
                              key={option.value}
                              value={String(option.value)}
                            >
                              {option.label}
                            </option>
                          ))}
                        </select>
                      </div>
                      <div>
                        <Label>Created</Label>
                        <p className="rounded-md border border-border bg-muted px-3 py-2 text-sm text-muted-foreground">
                          {new Date(request.createdAtUtc).toLocaleString()}
                        </p>
                      </div>
                    </div>
                    {request.comment ? (
                      <div className="rounded-md border border-border/80 bg-muted/80 p-3 text-sm text-foreground">
                        {request.comment}
                      </div>
                    ) : null}
                  </CardContent>
                  <CardFooter className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <Button
                      variant="destructive"
                      size="sm"
                      disabled={activeRequestId === request.id}
                      onClick={() => void handleDeleteRequest(request.id)}
                    >
                      <Trash2 className="mr-2 h-4 w-4" />
                      Delete
                    </Button>
                    {activeRequestId === request.id ? (
                      <span className="text-sm text-muted-foreground">
                        Updating...
                      </span>
                    ) : null}
                  </CardFooter>
                </Card>
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
