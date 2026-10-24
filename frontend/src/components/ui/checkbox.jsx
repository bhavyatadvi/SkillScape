import * as React from "react";
import * as CheckboxPrimitive from "@radix-ui/react-checkbox";

const Checkbox = React.forwardRef(({ className, checked, ...props }, ref) => (
  <CheckboxPrimitive.Root ref={ref} checked={checked} {...props} />
));
Checkbox.displayName = "Checkbox";

export { Checkbox };
