import React from 'react';

interface HeaderProps {
  title: string;
  actionButton?: {
    label: string;
    onClick: () => void;
  };
}

export const Header: React.FC<HeaderProps> = ({ title, actionButton }) => {
  return (
    <header className="top-bar">
      <h2 className="page-title">{title}</h2>
      {actionButton && (
        <button className="btn btn-primary" onClick={actionButton.onClick}>
          {actionButton.label}
        </button>
      )}
    </header>
  );
};
