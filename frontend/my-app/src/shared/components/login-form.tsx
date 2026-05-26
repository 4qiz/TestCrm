"use client";

import { useState, useTransition, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { cn } from "../lib/utils";
import { Button } from "../ui/button";
import { Loader2, TriangleAlert } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "../ui/card";
import { Label } from "../ui/label";
import { Input } from "../ui/input";
import { PasswordInput } from "./password-input";
import { appRoutes } from "../constants/app-routes";
import { apiRoutes } from "../constants/api-routes";

export const LoginForm = () => {
  const router = useRouter();
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    startTransition(() => {
      void (async () => {
        try {
          const response = await fetch(apiRoutes.login, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            credentials: "include",
            body: JSON.stringify({ login, password }),
          });

          if (response.ok) {
            router.push(appRoutes.home);
            return;
          }

          let message = "Не удалось войти. Проверьте имя и пароль.";
          const contentType = response.headers.get("content-type");

          if (contentType?.includes("application/json")) {
            const data = await response.json();
            if (data?.message) {
              message = data.message;
            } else if (data?.error) {
              message = data.error;
            }
          } else {
            const text = await response.text();
            if (text) {
              message = text;
            }
          }

          setError(message);
        } catch (err) {
          setError("Произошла ошибка сети. Попробуйте снова.");
        }
      })();
    });
  };

  return (
    <section className="h-full flex fixed inset-0 md:justify-center ">
      {/* Centered Login Card */}
      <div className=" w-full grid place-items-stretch md:place-items-center md:px-4 ">
        <Card
          className="py-0 md:py-6 h-fit w-full md:max-w-md rounded-none md:rounded-xl z-10
         
        backdrop-blur-none md:backdrop-blur-xl md:shadow-2xl"
        >
          <CardHeader className="pt-20 md:pt-0 ">
            <CardTitle className="flex justify-between">
              <h1
                className="text-2xl md:text-3xl font-bold  
              bg-clip-text"
              >
                Вход
              </h1>
            </CardTitle>
          </CardHeader>

          <form onSubmit={handleSubmit}>
            <CardContent className="space-y-6">
              <div className="grid gap-2">
                <Label htmlFor="login" className="">
                  Логин
                </Label>
                <div className="relative">
                  <Input
                    id="login"
                    name="login"
                    value={login}
                    onChange={(event) => setLogin(event.target.value)}
                    className="border text-lg "
                  />
                </div>
              </div>
              <div className="grid gap-2">
                <Label htmlFor="password" className="">
                  Пароль
                </Label>
                <PasswordInput
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  className="border text-lg backdrop-blur-xl"
                />
              </div>

              <Button
                disabled={isPending}
                aria-disabled={isPending}
                className={cn(
                  "w-full font-semibold border backdrop-blur-xl",
                  isPending && "cursor-not-allowed opacity-80",
                )}
                type="submit"
              >
                <span className="flex items-center justify-center gap-2">
                  {isPending && <Loader2 className="h-4 w-4 animate-spin" />}

                  <span
                    className={cn(
                      "transition-all duration-200",
                      isPending && "opacity-90",
                    )}
                  >
                    {isPending ? "" : "Вход"}
                  </span>
                </span>

                {/* subtle shimmer overlay */}
                {isPending && (
                  <span className="absolute inset-0 overflow-hidden rounded-md">
                    <span className="absolute inset-0 animate-[shimmer_1.5s_infinite] bg-linear-to-r from-transparent via-white/10 to-transparent" />
                  </span>
                )}
              </Button>

              {error && (
                <div className="flex items-center gap-1 text-sm text-destructive">
                  <TriangleAlert />
                  {error}
                </div>
              )}
            </CardContent>
          </form>
        </Card>
      </div>
    </section>
  );
};
