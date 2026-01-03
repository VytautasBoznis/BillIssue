import flattenDeep from 'lodash/flattenDeep';
import React from 'react';
import { Route, Routes as ReactRoutes } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import { getUserJwtToken } from '../utils/sessionUtils';

const generateFlattenRoutes = (routes) => {
  if (!routes) return [];
  return flattenDeep(routes.map(({ routes: subRoutes, ...rest }) => [rest, generateFlattenRoutes(subRoutes)]));
}

const isAuthorized = () => {
  return !!getUserJwtToken();
}

export const renderRoutes = (mainRoutes) => {
  const Routes = () => {
    const layouts = mainRoutes.map(({ layout: Layout, routes, isPublic }, index) => {
      const subRoutes = generateFlattenRoutes(routes);

      return (
        <Route key={index} element={<Layout />}>
          <Route element={<ProtectedRoute isAuthorized={isAuthorized} isPublic={isPublic}/>}>
            {subRoutes.map(({ component: Component, path, name }) => {
              return (
                Component && path && (<Route key={name} element={<Component />} path={path} />)
              )
            })}
          </Route>
        </Route>
      )
    });
    return <ReactRoutes>{layouts}</ReactRoutes>;
  }
  return Routes;
}