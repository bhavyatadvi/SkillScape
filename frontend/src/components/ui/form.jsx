import * as React from "react";
import { useForm as useReactHookForm } from "react-hook-form";

export function useForm(options) {
  return useReactHookForm(options);
}

export const Form = ({ children, onSubmit }) => (
  <form onSubmit={onSubmit}>{children}</form>
);
