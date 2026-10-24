import * as React from "react";
import * as PopoverPrimitive from "@radix-ui/react-popover";

import { cn } from "@/lib/utils";

const Popover = PopoverPrimitive.Root;
const PopoverTrigger = PopoverPrimitive.Trigger;
const PopoverContent = React.forwardRef(({ className, side = "right", ...props }, ref) => (
  <PopoverPrimitive.Content ref={ref} className={cn("z-50 w-56 rounded-lg border bg-popover p-2", className)} side={side} {...props} />
));
PopoverContent.displayName = "PopoverContent";

export { Popover, PopoverTrigger, PopoverContent };
