export type RequestStatus = 0 | 1 | 2 | 3;

export interface RequestDto {
  id: string;
  clientName: string;
  phone: string;
  status: RequestStatus;
  comment?: string | null;
  createdAtUtc: string;
}

export interface CreateRequestRequest {
  clientName: string;
  phone: string;
  comment?: string;
}

export interface UpdateRequestStatusRequest {
  status: RequestStatus;
}
