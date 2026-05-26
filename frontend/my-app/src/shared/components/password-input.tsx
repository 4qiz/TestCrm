"use client";

import { Input } from "@/shared/ui/input";
import { Eye, EyeOff } from "lucide-react";
import { useState, type ChangeEventHandler } from "react";

export const PasswordInput = ({
  value,
  onChange,
  className,
}: {
  value?: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  className?: string;
}) => {
  const [showPassword, setShowPassword] = useState(false);
  return (
    <div className="relative">
      <Input
        value={value}
        onChange={onChange}
        id="password"
        name="password"
        type={showPassword ? "text" : "password"}
        className={className}
      />
      <button
        type="button"
        className="absolute right-2 top-1/2 -translate-y-1/2 p-2 rounded-md cursor-pointer   "
        onClick={() => setShowPassword((v) => !v)}
      >
        {showPassword ? (
          <EyeOff className="h-4 w-4" />
        ) : (
          <Eye className="h-4 w-4" />
        )}
      </button>
    </div>
  );
};
