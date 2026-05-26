export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "";

export const apiRoutes = {
  login: `${API_BASE_URL}/api/account/login`,
  refresh: `${API_BASE_URL}/api/account/refresh`,
};
