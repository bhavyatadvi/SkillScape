import React from 'react';
import { Link } from 'react-router-dom';
import { Alert, AlertTitle, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';

const NotFound = () => (
  <div className="flex min-h-screen items-center justify-center">
    <div className="w-full max-w-md p-6">
      <Alert>
        <AlertTitle>404 - Not Found</AlertTitle>
        <AlertDescription>The page you are looking for does not exist.</AlertDescription>
      </Alert>

      <div className="mt-6 text-center">
        <Link to="/dashboard">
          <Button className="w-full">Go to Dashboard</Button>
        </Link>
      </div>
    </div>
  </div>
);

export default NotFound;
