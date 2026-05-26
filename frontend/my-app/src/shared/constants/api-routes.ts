export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "";
const REQUESTS_BASE_URL = `${API_BASE_URL}/api/Requests`;

export const apiRoutes = {
  login: `${API_BASE_URL}/api/account/login`,
  refresh: `${API_BASE_URL}/api/account/refresh`,
  requests: REQUESTS_BASE_URL,
  request: (id: string) => `${REQUESTS_BASE_URL}/${id}`,
  updateRequestStatus: (id: string) => `${REQUESTS_BASE_URL}/${id}/status`,
  deleteRequest: (id: string) => `${REQUESTS_BASE_URL}/${id}`,
  requestsHub: `${API_BASE_URL}/hubs/requests`,
};
