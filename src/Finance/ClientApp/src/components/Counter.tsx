import React, { useState } from "react";
import { Button, Header } from "semantic-ui-react";

export function Counter() {
  var [currentCount, setCurrentCount] = useState(0);

  function incrementCounter() {
    setCurrentCount(currentCount + 1);
  }
  return (
    <div>
      <Header>Counter</Header>

      <p>This is a simple example of a React component.</p>

      <p>
        Current count: <strong>{currentCount}</strong>
      </p>

      <Button onClick={incrementCounter}>
        Increment
      </Button>
    </div>
  );
}
