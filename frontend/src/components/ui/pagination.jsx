import * as React from "react";

const Pagination = ({ children, className, ...props }) => (
  <nav className={className} {...props} aria-label="Pagination">
    {children}
  </nav>
);

export { Pagination };
