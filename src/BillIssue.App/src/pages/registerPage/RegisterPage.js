import React from 'react';
import { useState } from "react";
import { useForm } from "react-hook-form";
import AuthService from "../../services/AuthService";
import SpinnerIcon from "../../components/icons/SpinnerIcon";

import "./RegisterPage.css";

const RegisterPage = () => {
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm();

  const onSubmit = async (data) => {
    setLoading(true);
    try {
      await AuthService.register(data.email, data.firstName, data.lastName, data.password, ()=>{} );
      setLoading(false);
      window.location.href = '/';
    } catch (error) {
      setLoading(false);
      setMessage(error.message);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div className="container">
        <div className="register-container">
          <div className="text-center mb-4">
            <h4>Registration</h4>
          </div>
          <div className="mb-3">
            <div className="d-flex justify-content-between">
              <div className="input-label">Email</div>
            </div>
            <input
              className="form-control col-sm-12"
              placeholder="Email"
              {...register("email", { required: true })}/>
              <div className="col-sm-12">
                {errors.email && <span className="error-label">Email is required</span>}
              </div>
          </div>
          <div className="mb-3">
            <div className="d-flex justify-content-between">
              <div className="input-label">First name</div>
            </div>
            <input
              type="text"
              placeholder="First name"
              className="form-control col-sm-12"
              {...register("firstName", { required: true })}
            />
            <div className="col-sm-12">
              {errors.firstName && <span className="error-label">First name is required</span>}
            </div>
          </div>          
          <div className="mb-3">
            <div className="d-flex justify-content-between">
              <div className="input-label">Last name</div>
            </div>
            <input
              type="text"
              placeholder="Last name"
              className="form-control col-sm-12"
              {...register("lastName", { required: true })}
            />
            <div className="col-sm-12">
              {errors.lastName && <span className="error-label">Last name is required</span>}
            </div>
          </div>
          <div className="mb-3">
            <div className="d-flex justify-content-between">
              <div className="input-label">Password</div>
            </div>
            <input
              type="password"
              placeholder="Password"
              className="form-control col-sm-12"
              {...register("password", { 
                required: "Password is required",
                minLength: {
                  value: 8,
                  message: "Password must have at least 8 characters"
                }, })}
            />
            <div className="col-sm-12">
              {errors.password && <span className="error-label">{errors.password.message}</span>}
            </div>
          </div>
          <div className="mb-3">
            <div className="d-flex justify-content-between">
              <div className="input-label">Repeat password</div>
            </div>
            <input
              type="password"
              placeholder="Repeat password"
              className="form-control col-sm-12"
              {...register("repeatPassword", { 
                required: true,
                validate: (val) => {
                if (watch('password') !== val) {
                  return "Your passwords do no match";
                }
              }, })}
            />
            <div className="col-sm-12">
              {errors.repeatPassword && <span className="error-label">Password needs to be repeated and needs to match</span>}
            </div>
          </div>
          <div className="mb-3">
            <button type="submit" className="btn btn-primary register-button">
              { loading ? (<SpinnerIcon classes="loader-spin"/>) : ("Register") }
            </button>
          </div>
          <div className="mb-3">
            <div className="text-center">
              <div className="subheader-label">Already have an account? <a href="/login">Login</a></div>
            </div>
          </div>
          <div className="text-center error-text">
            {message && <p>{message}</p>}
          </div>
        </div>
      </div>
    </form>
  )
}

export default RegisterPage;
